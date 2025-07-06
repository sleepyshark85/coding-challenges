using Elsa.QuizAPI.Data;
using Elsa.QuizAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Elsa.QuizAPI.Features.Quizzes;

public interface IQuizRepository
{
    Task<Quiz?> GetQuizAsync(Guid quizId, CancellationToken cancellationToken = default);
    Task<UserQuiz?> GetUserQuizAsync(Guid userId, Guid quizId, CancellationToken cancellationToken = default);
    Task<UserQuiz> AddUserQuizAsync(UserQuiz userQuiz, CancellationToken cancellationToken = default);
    Task<UserQuiz> UpdateUserQuizAsync(UserQuiz userQuiz, CancellationToken cancellationToken = default);
}

public class QuizRepository : IQuizRepository
{
    private readonly QuizDbContext _context;
    private readonly ILogger<QuizRepository> _logger;

    public QuizRepository(QuizDbContext context, ILogger<QuizRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Quiz?> GetQuizAsync(Guid quizId, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.QuizId == quizId, cancellationToken);
    }

    public async Task<UserQuiz?> GetUserQuizAsync(Guid userId, Guid quizId, CancellationToken cancellationToken = default)
    {
        return await _context.UserQuizzes
            .Include(q => q.QuestionAttempts)
            .FirstOrDefaultAsync(q => q.UserId == userId && q.QuizId == quizId);
    }

    public async Task<UserQuiz> AddUserQuizAsync(UserQuiz userQuiz, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _context.UserQuizzes.Add(userQuiz);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return userQuiz;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "User {UserId} failed to join quiz {QuizId}", userQuiz.UserId, userQuiz.QuizId);
            throw;
        }
    }

    public async Task<UserQuiz> UpdateUserQuizAsync(UserQuiz userQuiz, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return userQuiz;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "User {UserId} failed to join quiz {QuizId}", userQuiz.UserId, userQuiz.QuizId);
            throw;
        }
    }
}