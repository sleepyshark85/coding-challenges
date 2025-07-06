namespace Elsa.QuizAPI.Domain.Models;

public class UserQuiz
{
    private readonly List<UserQuizQuestion> _questionAttempts = new();
    
    private UserQuiz() { }
    
    public UserQuiz(Guid userId, Quiz quiz)
    {
        if (Guid.Empty.Equals(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (quiz is null)
            throw new ArgumentException("Invalid quiz", nameof(quiz));
            
        UserId = userId;
        QuizId = quiz.QuizId;
        StartedAt = DateTime.UtcNow;
        ExpiresAt = StartedAt.Add(quiz.TimeLimit);
        Status = UserQuizStatus.InProgress;
        
        foreach (var question in quiz.Questions)
        {
            var questionAttempt = new UserQuizQuestion(question);
            _questionAttempts.Add(questionAttempt);
        }
    }

    public Guid UserQuizId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid QuizId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public UserQuizStatus Status { get; private set; }
    
    public IReadOnlyList<UserQuizQuestion> QuestionAttempts => _questionAttempts.AsReadOnly();

    public int TotalPointsEarned => QuestionAttempts.Where(q => q.IsCorrect).Sum(q => q.PointsEarned);
    
    public UserQuizQuestion? GetQuestionAttempt(Guid questionId)
    {
        return _questionAttempts.FirstOrDefault(qa => qa.QuestionId == questionId);
    }

    public UserQuizQuestion? GetNextUnansweredQuestion()
    {
        return _questionAttempts
            .Where(qa => !qa.IsAnswered)
            .FirstOrDefault();
    }

    public IEnumerable<UserQuizQuestion> GetAnsweredQuestions()
    {
        return _questionAttempts.Where(qa => qa.IsAnswered);
    }

    public void SubmitAnswer(QuizQuestion question, string answer)
    {
        var questionAttempt = GetQuestionAttempt(question.QuestionId);
        if (questionAttempt == null)
            throw new InvalidOperationException($"Question {question.QuestionId} not found in quiz attempt");
            
        questionAttempt.SubmitAnswer(answer, question);
    }

    public void Complete()
    {
        if (Status != UserQuizStatus.InProgress)
            throw new InvalidOperationException("Quiz is not in progress");
            
        Status = UserQuizStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Abandon()
    {
        if (Status != UserQuizStatus.InProgress)
            throw new InvalidOperationException("Quiz is not in progress");
            
        Status = UserQuizStatus.Abandoned;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        if (Status == UserQuizStatus.InProgress)
        {
            Status = UserQuizStatus.Expired;
            CompletedAt = DateTime.UtcNow;
        }
    }
}

public enum UserQuizStatus
{
    InProgress = 0,
    Completed = 1,
    Abandoned = 2,
    Expired = 3
}