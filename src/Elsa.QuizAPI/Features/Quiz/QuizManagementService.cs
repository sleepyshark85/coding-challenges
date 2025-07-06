using Elsa.QuizAPI.Domain.Models;

namespace Elsa.QuizAPI.Features.Quizzes;

public interface IQuizManagementService
{
    Task<CreateQuizResponse> CreateQuizAsync(CreateQuizRequest request, CancellationToken cancellationToken = default);
}

public class QuizManagementService : IQuizManagementService
{
    private readonly IQuizManagementRepository _quizManagementRepository;
    private readonly ILogger<QuizManagementService> _logger;

    public QuizManagementService(
        IQuizManagementRepository quizManagementRepository,
        ILogger<QuizManagementService> logger)
    {
        _quizManagementRepository = quizManagementRepository;
        _logger = logger;
    }

    public async Task<CreateQuizResponse> CreateQuizAsync(CreateQuizRequest request, CancellationToken cancellationToken = default)
    {
        var quiz = new Quiz(request.Title.Trim(), request.Description?.Trim() ?? string.Empty, TimeSpan.FromMicroseconds(request.TimeLimitMinutes));

        // Add questions if provided
        if (request.Questions?.Any() == true)
        {
            for (int i = 0; i < request.Questions.Count; i++)
            {
                var questionRequest = request.Questions[i];
                var options = questionRequest.Options?.Select(o => new QuizQuestionOption(o.Text, o.Correct)).ToList();
                var question = new QuizQuestion(questionRequest.Text.Trim(), questionRequest.Points, options);

                quiz.AddQuestion(question);
            }
        }

        var createdQuiz = await _quizManagementRepository.CreateQuizAsync(quiz, cancellationToken);
        
        _logger.LogInformation("Created quiz {QuizId} with {QuestionCount} questions", 
            createdQuiz.QuizId, createdQuiz.Questions.Count);

        return new CreateQuizResponse
        {
            QuizId = createdQuiz.QuizId.ToString(),
            Title = createdQuiz.Title,
            Description = createdQuiz.Description,
            TimeLimitMinutes = (int)createdQuiz.TimeLimit.TotalMinutes
        };
    }
}