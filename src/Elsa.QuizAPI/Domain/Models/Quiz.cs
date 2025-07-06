using System.Net.Quic;
using System.Text.Json.Serialization;

namespace Elsa.QuizAPI.Domain.Models;
public class Quiz
{
    private Quiz() { }
    public Quiz(string title, string description, TimeSpan timeLimit)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        if (timeLimit.TotalSeconds <= 0)
            throw new ArgumentException("Invalid time limit", nameof(timeLimit));

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        TimeLimit = timeLimit;
    }

    public Guid QuizId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TimeSpan TimeLimit { get; init; }

    // private readonly List<QuizQuestion> _questions = new();

    //Should use backing field instead for better encapsulation, 
    // but the json serialization is a little bit complicate to implement for this sample project
    public List<QuizQuestion> Questions { get; init; } = new();
    
    public void AddQuestion(QuizQuestion question)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));
        if (question.QuizId != QuizId)
            throw new ArgumentException("Question belongs to different quiz");
        if (Questions.Any(q => q.QuestionId == question.QuestionId))
            throw new ArgumentException("Question already exists in quiz");
            
        Questions.Add(question);
    }

    public QuizQuestion? GetQuestion(Guid questionId)
    {
        return Questions.FirstOrDefault(q => q.QuestionId == questionId);
    }

    public void RemoveQuestion(Guid questionId)
    {
        var question = Questions.FirstOrDefault(q => q.QuestionId == questionId);
        if (question != null)
        {
            Questions.Remove(question);
        }
    }

    public UserQuiz CreateUserQuiz(Guid userId)
    {
        return new UserQuiz(userId, this);
    }
}