using Elsa.QuizAPI.Data;
using Elsa.QuizAPI.Domain.Models;

namespace Elsa.QuizAPI.Features.Quizzes;

public interface IQuizManagementRepository
{
    Task<Quiz> CreateQuizAsync(Quiz quiz, CancellationToken cancellationToken = default);
}

public class QuizManagementRepository : IQuizManagementRepository
{
    private readonly QuizDbContext _context;
    private readonly ILogger<QuizManagementRepository> _logger;

    public QuizManagementRepository(QuizDbContext context, ILogger<QuizManagementRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Quiz> CreateQuizAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Add quiz
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Successfully created quiz {QuizId} with {QuestionCount} questions", 
                quiz.QuizId, quiz.Questions.Count);

            return quiz;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create quiz {QuizId}", quiz.QuizId);
            throw;
        }
    }
}