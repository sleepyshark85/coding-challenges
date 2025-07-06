using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Elsa.QuizAPI.Features.Quizzes;


/// <summary>
/// Quiz Management API - Handles quiz creation, updates, and administrative operations
/// </summary>
[ApiController]
[Route("api/v1/quiz-management")]
[Produces("application/json")]
[Tags("Quiz Management")]
public class QuizManagementController : ControllerBase
{
    private readonly IQuizManagementService _quizManagementService;
    private readonly ILogger<QuizManagementController> _logger;

    public QuizManagementController(
        IQuizManagementService quizManagementService,
        ILogger<QuizManagementController> logger)
    {
        _quizManagementService = quizManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new quiz with questions
    /// </summary>
    /// <param name="request">Quiz creation request containing title, description, questions, and settings</param>
    /// <param name="cancellationToken">Cancellation token for request cancellation</param>
    /// <returns>Created quiz details with assigned IDs</returns>
    /// <remarks>
    /// Creates a new quiz with all associated questions in a single atomic operation.
    /// </remarks>
    /// <response code="201">Quiz created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateQuizResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<CreateQuizResponse>> CreateQuiz(
        [FromBody] CreateQuizRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating quiz with title: {Title}", request.Title);

            var result = await _quizManagementService.CreateQuizAsync(request, cancellationToken);

            _logger.LogInformation("Quiz created successfully with ID: {QuizId}", result.QuizId);

            return Created(string.Empty, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid request for quiz creation: {Message}", ex.Message);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Quiz creation cancelled");
            return StatusCode((int)HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create quiz");
            return StatusCode((int)HttpStatusCode.InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while creating the quiz",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }
}

/// <summary>
/// Request model for creating a new quiz with questions
/// </summary>
public class CreateQuizRequest
{
    /// <summary>
    /// Quiz title (required, 3-200 characters)
    /// </summary>
    /// <example>English Vocabulary Quiz</example>
    [Required(ErrorMessage = "Quiz title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Quiz description (optional, max 1000 characters)
    /// </summary>
    /// <example>Test your English vocabulary knowledge with challenging questions</example>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Time limit for quiz completion in minutes (required, 1-480 minutes)
    /// </summary>
    /// <example>30</example>
    [Required(ErrorMessage = "Time limit is required")]
    [Range(1, 480, ErrorMessage = "Time limit must be between 1 and 480 minutes (8 hours)")]
    public int TimeLimitMinutes { get; set; }

    /// <summary>
    /// List of questions for the quiz (optional, max 100 questions)
    /// </summary>
    public List<CreateQuestionRequest>? Questions { get; set; }
}

/// <summary>
/// Request model for creating a quiz question
/// </summary>
public class CreateQuestionRequest
{
    /// <summary>
    /// Question text (required, 10-1000 characters)
    /// </summary>
    /// <example>What is the meaning of 'ubiquitous'?</example>
    [Required(ErrorMessage = "Question text is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Question text must be between 10 and 1000 characters")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Points awarded for correct answer (required, 1-1000 points)
    /// </summary>
    /// <example>10</example>
    [Required(ErrorMessage = "Points value is required")]
    [Range(1, 1000, ErrorMessage = "Points must be between 1 and 1000")]
    public int Points { get; set; }

    /// <summary>
    /// Multiple choice options
    /// </summary>
    public List<CreateQuestionOptionRequest>? Options { get; set; }
}

/// <summary>
/// Request model for creating a quiz question option
/// </summary>
public class CreateQuestionOptionRequest
{
    /// <summary>
    /// Option text
    /// </summary>
    [Required(ErrorMessage = "Option text is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Option text must be between 10 and 1000 characters")]
    public string Text { get; set; }

    /// <summary>
    /// Indicate if the option text is the correct answer for the question
    /// </summary>
    public bool Correct { get; set; }
}

/// <summary>
/// Response model for quiz creation
/// </summary>
public class CreateQuizResponse
{
    /// <summary>
    /// Unique identifier for the created quiz
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public string QuizId { get; set; } = string.Empty;

    /// <summary>
    /// Quiz title
    /// </summary>
    /// <example>English Vocabulary Quiz</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Quiz description
    /// </summary>
    /// <example>Test your English vocabulary knowledge</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Time limit in minutes
    /// </summary>
    /// <example>30</example>
    public int TimeLimitMinutes { get; set; }

    /// <summary>
    /// List of created questions with their IDs
    /// </summary>
    public List<CreatedQuestionResponse> Questions { get; set; } = new();
}

/// <summary>
/// Response model for a created question
/// </summary>
public class CreatedQuestionResponse
{
    /// <summary>
    /// Unique identifier for the question
    /// </summary>
    /// <example>456e7890-e12b-34c5-d678-901234567890</example>
    public string QuestionId { get; set; } = string.Empty;

    /// <summary>
    /// Question text
    /// </summary>
    /// <example>What is the meaning of 'ubiquitous'?</example>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Points for this question
    /// </summary>
    /// <example>10</example>
    public int Points { get; set; }
}