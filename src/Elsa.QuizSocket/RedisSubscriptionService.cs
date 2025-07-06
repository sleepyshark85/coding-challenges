using System.Text.Json;
using Elsa.QuizSocket.Events;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Elsa.QuizSocket;

public interface IRedisSubscriptionService
{
    Task StartAsync();
    Task StopAsync();
}

public class RedisSubscriptionService : IRedisSubscriptionService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IHubContext<QuizHub> _hubContext;
    private readonly IQuizConnectionManager _connectionManager;
    private readonly ILogger<RedisSubscriptionService> _logger;
    private ISubscriber? _subscriber;

    public RedisSubscriptionService(
        IConnectionMultiplexer redis,
        IHubContext<QuizHub> hubContext,
        IQuizConnectionManager connectionManager,
        ILogger<RedisSubscriptionService> logger)
    {
        _redis = redis;
        _hubContext = hubContext;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        try
        {
            _subscriber = _redis.GetSubscriber();
            
            // Subscribe to points updates pattern
            await _subscriber.SubscribeAsync(
                new RedisChannel("quiz:*:points_updates", RedisChannel.PatternMode.Pattern),
                async (channel, message) =>
                {
                    try
                    {
                        var scoreUpdate = JsonSerializer.Deserialize<QuizQuestionAnsweredEvent>(message);
                        if (scoreUpdate != null)
                        {
                            await HandlePointsUpdateForUserAsync(scoreUpdate);
                            await HandlePointsUpdateForLeaderboardAsync(scoreUpdate);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing points update from Redis");
                    }
                });

            _logger.LogInformation("Redis subscription service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Redis subscription service");
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (_subscriber != null)
        {
            await _subscriber.UnsubscribeAllAsync();
            _logger.LogInformation("Redis subscription service stopped");
        }
    }

    /// <summary>
    /// This will send an updated event to the user connection
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    private async Task HandlePointsUpdateForUserAsync(QuizQuestionAnsweredEvent @event)
    {
        try
        {
            var connection = _connectionManager.GetUserConnection(@event.UserId.ToString());

            if (connection is not null)
            {
                await _hubContext.Clients.Client(connection.ConnectionId).SendAsync("UserPointsUpdated", new
                {
                    UserId = @event.UserId,
                    QuizId = @event.QuizId,
                    QuestionId = @event.QuestionId,
                    IsCorrect = @event.IsCorrect,
                    PointsEarned = @event.PointsEarned,
                    TotalPointsEarned = @event.TotalPointsEarned
                });

                _logger.LogDebug($"Points update broadcasted to {connection.ConnectionId} for quiz {@event.QuizId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling score update");
        }
    }

    /// <summary>
    /// This will send an updated event to all users that are connecting to a quiz
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    private async Task HandlePointsUpdateForLeaderboardAsync(QuizQuestionAnsweredEvent @event)
    {
        try
        {
            var connections = _connectionManager.GetConnectionsForQuiz(@event.QuizId.ToString());

            if (connections.Any())
            {
                await _hubContext.Clients.Clients(connections).SendAsync("LeaderboardUpdated", new
                {
                    UserId = @event.UserId,
                    TotalPointsEarned = @event.TotalPointsEarned
                });

                _logger.LogDebug($"Points update broadcasted to {connections.Count()} connections for quiz {@event.QuizId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling score update");
        }
    }
}