using System.Collections.Concurrent;

namespace Elsa.QuizSocket;

public interface IQuizConnectionManager
{
    Task AddToQuizAsync(string connectionId, string quizId, string userId);
    Task RemoveFromQuizAsync(string connectionId, string quizId);
    Task RemoveConnectionAsync(string connectionId);
    IEnumerable<string> GetConnectionsForQuiz(string quizId);
    string? GetQuizIdForConnection(string connectionId);
    QuizConnection? GetUserConnection(string userId);
    QuizConnection? GetConnection(string connectionId);
}

public class QuizConnectionManager : IQuizConnectionManager
{
    private readonly ConcurrentDictionary<string, QuizConnection> _connections = new();
    private readonly ConcurrentDictionary<string, QuizConnection> _userConnections = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _quizConnections = new();
    private readonly ILogger<QuizConnectionManager> _logger;

    public QuizConnectionManager(ILogger<QuizConnectionManager> logger)
    {
        _logger = logger;
    }

    public async Task AddToQuizAsync(string connectionId, string quizId, string userId)
    {
        var connection = new QuizConnection
        {
            ConnectionId = connectionId,
            UserId = userId,
            QuizId = quizId
        };

        _connections.AddOrUpdate(connectionId, connection, (key, oldValue) => connection);
        _userConnections.AddOrUpdate(userId, connection, (key, oldValue) => connection);

        _quizConnections.AddOrUpdate(quizId,
            new HashSet<string> { connectionId },
            (key, existingConnections) =>
            {
                existingConnections.Add(connectionId);
                return existingConnections;
            });

        _logger.LogInformation($"User {userId} connected to quiz {quizId} with connection {connectionId}");

        await Task.CompletedTask;
    }

    public async Task RemoveFromQuizAsync(string connectionId, string quizId)
    {
        if (_quizConnections.TryGetValue(quizId, out var connections))
        {
            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                _quizConnections.TryRemove(quizId, out _);
            }
        }

        _logger.LogInformation($"Connection {connectionId} removed from quiz {quizId}");

        await Task.CompletedTask;
    }

    public async Task RemoveConnectionAsync(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            await RemoveFromQuizAsync(connectionId, connection.QuizId);
            _logger.LogInformation($"Connection {connectionId} removed completely");
        }
    }

    public IEnumerable<string> GetConnectionsForQuiz(string quizId)
    {
        return _quizConnections.TryGetValue(quizId, out var connections)
            ? connections.ToList()
            : Enumerable.Empty<string>();
    }

    public string? GetQuizIdForConnection(string connectionId)
    {
        return _connections.TryGetValue(connectionId, out var connection)
            ? connection.QuizId
            : null;
    }

    public QuizConnection? GetUserConnection(string userId)
    {
        return _userConnections.TryGetValue(userId, out var connection)
            ? connection
            : null;
    }

    public QuizConnection? GetConnection(string connectionId)
    {
        return _connections.TryGetValue(connectionId, out var connection)
            ? connection
            : null;
    }
}

public class QuizConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}