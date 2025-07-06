namespace Elsa.QuizSocket.Events;

public class QuizQuestionAnsweredEvent
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public int TotalPointsEarned { get; set; }
}