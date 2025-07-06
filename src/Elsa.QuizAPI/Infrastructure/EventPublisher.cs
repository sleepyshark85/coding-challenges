using System.Text.Json;
using StackExchange.Redis;

namespace Elsa.QuizAPI.Infrastructure;

public interface IEventPublisher
{
    Task PublishEventAsync(IQuizEvent @event);
}

public class RedisEventPublisher : IEventPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisEventPublisher> _logger;
    
    public RedisEventPublisher(IConnectionMultiplexer redis, ILogger<RedisEventPublisher> logger)
    {
        _redis = redis;
        _logger = logger;
    }
    
    public async Task PublishEventAsync(IQuizEvent @event)
    {
        try
        {
            var database = _redis.GetDatabase();
            var channel = new RedisChannel($"quiz:{@event.QuizId}:points_updates", RedisChannel.PatternMode.Pattern);
            var message = JsonSerializer.Serialize(@event, @event.GetType());
            
            await database.PublishAsync(channel, message);
            _logger.LogInformation($"Event published for user {@event.UserId} in quiz {@event.QuizId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event");
        }
    }
}

public interface IQuizEvent
{
    Guid UserId { get; set; }
    Guid QuizId { get; set; }
}


public class QuizQuestionAnsweredEvent : IQuizEvent
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public int TotalPointsEarned { get; set; }
}