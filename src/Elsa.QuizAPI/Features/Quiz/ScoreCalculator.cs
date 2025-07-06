// using Elsa.QuizAPI.Data;

// namespace Elsa.QuizAPI.Features.Quizzes;

// public interface IScoreCalculator
// {
//     Task<int> CalculateScoreAsync(Quiz quiz, Question question, AnswerSubmission submission, UserScore userScore);
// }

// public class ScoreCalculator : IScoreCalculator
// {
//     public async Task<int> CalculateScoreAsync(Quiz quiz, Question question, AnswerSubmission submission, UserScore userScore)
//     {
//         if (!IsAnswerCorrect(question, submission.Answer))
//         {
//             return 0;
//         }
        
//         return quiz.ScoringType switch
//         {
//             ScoringType.Simple => question.Points,
//             ScoringType.TimeBased => CalculateTimeBasedScore(question, submission.ResponseTime),
//             ScoringType.StreakBased => await CalculateStreakBasedScore(question, userScore),
//             _ => question.Points
//         };
//     }
    
//     private bool IsAnswerCorrect(Question question, string submittedAnswer)
//     {
//         return string.Equals(question.CorrectAnswer, submittedAnswer, StringComparison.OrdinalIgnoreCase);
//     }
    
//     private int CalculateTimeBasedScore(Question question, TimeSpan responseTime)
//     {
//         var basePoints = question.Points;
//         var timeLimit = TimeSpan.FromSeconds(30); // Default time limit
//         var timeBonus = Math.Max(0, timeLimit.TotalSeconds - responseTime.TotalSeconds);
//         var bonusPoints = (int)(timeBonus * 2); // 2 points per second saved
        
//         return Math.Max(10, basePoints + bonusPoints); // Minimum 10 points
//     }
    
//     private async Task<int> CalculateStreakBasedScore(Question question, UserScore userScore)
//     {
//         var basePoints = question.Points;
//         var streakMultiplier = Math.Min(3.0, 1.0 + (userScore.CorrectAnswers * 0.1)); // Max 3x multiplier
//         return (int)(basePoints * streakMultiplier);
//     }
// }