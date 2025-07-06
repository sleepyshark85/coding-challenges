using Microsoft.AspNetCore.SignalR;

namespace Elsa.QuizSocket;

public class QuizHub : Hub
{
    private readonly IQuizConnectionManager _connectionManager;
    private readonly ILogger<QuizHub> _logger;

    public QuizHub(IQuizConnectionManager connectionManager, ILogger<QuizHub> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task JoinQuiz(string quizId, string userId)
    {
        try
        {
            await _connectionManager.AddToQuizAsync(Context.ConnectionId, quizId, userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Quiz_{quizId}");
            
            _logger.LogInformation($"User {userId} joined quiz {quizId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error joining quiz {quizId} for user {userId}");
            await Clients.Caller.SendAsync("Error", "Failed to join quiz");
        }
    }

    public async Task LeaveQuiz(string quizId)
    {
        try
        {
            var connection = _connectionManager.GetConnection(Context.ConnectionId);
            if (connection != null)
            {
                await _connectionManager.RemoveFromQuizAsync(Context.ConnectionId, quizId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Quiz_{quizId}");
                
                _logger.LogInformation($"User {connection.UserId} left quiz {quizId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error leaving quiz {quizId}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var connection = _connectionManager.GetConnection(Context.ConnectionId);
            if (connection != null)
            {
                await _connectionManager.RemoveConnectionAsync(Context.ConnectionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when handling disconnection for {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}