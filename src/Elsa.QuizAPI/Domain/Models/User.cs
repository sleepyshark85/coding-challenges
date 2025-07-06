namespace Elsa.QuizAPI.Domain.Models;
public class User
{
    private readonly List<UserQuiz> _quizAttempts = new();
    
    private User() { }
    
    public User(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));
            
        Username = username.Trim();
    }

    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    
    public IReadOnlyList<UserQuiz> QuizAttempts => _quizAttempts.AsReadOnly();
}