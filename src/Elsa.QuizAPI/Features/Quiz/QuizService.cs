using Elsa.QuizAPI.Domain.Models;
using Elsa.QuizAPI.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Elsa.QuizAPI.Features.Quizzes;

public interface IQuizService
{
    Task<Quiz?> GetQuizAsync(Guid quizId, CancellationToken cancellationToken = default);
    Task<SubmissionResult> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest submission, CancellationToken cancellationToken = default);
    Task<JoinQuizResponse?> JoinQuizAsync(Guid userId, JoinQuizRequest request, CancellationToken cancellationToken = default);
}

public class QuizService : IQuizService
{
    private readonly HybridCache _cache;
    private readonly IQuizRepository _quizRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<QuizService> _logger;

    public QuizService(HybridCache cache, IQuizRepository quizRepository, IEventPublisher eventPublisher, ILogger<QuizService> logger)
    {
        _cache = cache;
        _quizRepository = quizRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Quiz?> GetQuizAsync(Guid quizId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"quiz:{quizId}";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token =>
            {
                _logger.LogInformation($"Quiz {quizId} loaded from database and cached");
                return await _quizRepository.GetQuizAsync(quizId, cancellationToken);
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(24),
                LocalCacheExpiration = TimeSpan.FromMinutes(30)
            },
            cancellationToken: cancellationToken);
    }

    public async Task<JoinQuizResponse?> JoinQuizAsync(Guid userId, JoinQuizRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        if (Guid.Empty.Equals(request.QuizId))
            throw new ArgumentException("Quiz ID is required");

        var quiz = await GetQuizAsync(request.QuizId);
        if (quiz is null)
            throw new ArgumentException(string.Format("Invalid quiz {QuizId}", request.QuizId));

        // Check if user already joined
        var userQuiz = await _quizRepository.GetUserQuizAsync(userId, request.QuizId);

        //Create user quiz session
        if (userQuiz == null)
        {
            userQuiz = quiz.CreateUserQuiz(userId);
            await _quizRepository.AddUserQuizAsync(userQuiz);
            _logger.LogInformation("User {UserId} joined quiz {QuizId}", userId, request.QuizId);
        }

        return new JoinQuizResponse
        {
            QuizId = quiz.QuizId,
            UserQuizId = userQuiz.UserQuizId,
            Title = quiz.Title,
            Description = quiz.Description,
            TimeLimit = quiz.TimeLimit
        };
    }

    public async Task<SubmissionResult> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest submission, CancellationToken cancellationToken = default)
    {
        try
        {
            var quiz = await GetQuizAsync(submission.QuizId);
            if (quiz == null)
                throw new InvalidOperationException("Quiz not found");

            var question = quiz.GetQuestion(submission.QuestionId);
            if (question == null)
                throw new InvalidOperationException("Question not found");

            var userQuiz = await _quizRepository.GetUserQuizAsync(userId, submission.QuizId);
            if (userQuiz is null)
                throw new ArgumentException(string.Format("User session not found for user {UserId} and quiz {QuizId}", userId, submission.QuizId));

            var attemptQuestion = userQuiz.GetQuestionAttempt(submission.QuestionId);
            if (attemptQuestion is null)
                throw new ArgumentException(string.Format("Question {QuestionId} not found for quiz session {UserQuizId}", submission.QuestionId, userQuiz.UserQuizId));

            attemptQuestion.SubmitAnswer(submission.Answer, question);

            await _quizRepository.UpdateUserQuizAsync(userQuiz, cancellationToken);

            //We only publish the event when there is a change in points
            if (attemptQuestion.IsCorrect)
            {
                await _eventPublisher.PublishEventAsync(new QuizQuestionAnsweredEvent
                {
                    UserId = userId,
                    QuizId = submission.QuizId,
                    QuestionId = submission.QuestionId,
                    IsCorrect = attemptQuestion.IsCorrect,
                    PointsEarned = attemptQuestion.PointsEarned,
                    TotalPointsEarned = userQuiz.TotalPointsEarned
                });
            }

            return new SubmissionResult
                {
                    UserQuizId = userQuiz.UserQuizId,
                    IsCorrect = attemptQuestion.IsAnswered && attemptQuestion.IsCorrect,
                    PointsEarned = attemptQuestion.PointsEarned,
                    TotalPointsEarned = userQuiz.TotalPointsEarned
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process answer submission");
            throw;
        }
    }
}