using System.ComponentModel.DataAnnotations;
using System.Net;
using Elsa.QuizAPI.Domain.Models;
using Elsa.QuizAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Elsa.QuizAPI.Features.Quizzes;

/// <summary>
/// Quiz Participation API - Handles user quiz interactions, joining, and answering
/// </summary>
[ApiController]
[Route("api/v1/quiz")]
[Produces("application/json")]
[Tags("Quiz Participation")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IUserContext _userContext;
    private readonly ILogger<QuizController> _logger;

    public QuizController(IQuizService quizService, IUserContext userContext, ILogger<QuizController> logger)
    {
        _quizService = quizService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Join a quiz session
    /// </summary>
    /// <param name="request">Join quiz request containing user and quiz information</param>
    /// <param name="cancellationToken">Cancellation token for request cancellation</param>
    /// <returns>Quiz session details</returns>
    /// <remarks>
    /// Allows a user to join an active quiz session. If the user has already joined,
    /// returns their existing state instead of creating a duplicate entry.
    /// </remarks>
    /// <response code="200">Successfully joined quiz or returned existing session</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Quiz not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("join")]
    [ProducesResponseType(typeof(JoinQuizResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<JoinQuizResponse>> JoinQuiz(
        [FromBody] JoinQuizRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("User {UserId} attempting to join quiz {QuizId}",
                _userContext.GetCurrentUser().UserId, request.QuizId);

            var result = await _quizService.JoinQuizAsync(_userContext.GetCurrentUser().UserId, request, cancellationToken);

            if (result == null)
            {
                _logger.LogWarning("Quiz {QuizId} not found or not active", request.QuizId);
                return NotFound(new ProblemDetails
                {
                    Title = "Quiz Not Found",
                    Detail = $"Quiz '{request.QuizId}' was not found or is not currently active",
                    Status = (int)HttpStatusCode.NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            _logger.LogInformation("User {UserId} successfully joined quiz {QuizId}.",
                _userContext.GetCurrentUser().UserId, request.QuizId);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid join request: {Message}", ex.Message);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid Join Request",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Quiz join operation cancelled");
            return StatusCode((int)HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to join quiz {QuizId} for user {UserId}",
                request.QuizId, _userContext.GetCurrentUser().UserId);
            return StatusCode((int)HttpStatusCode.InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while joining the quiz",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Submit an answer to a quiz question
    /// </summary>
    /// <param name="request">Answer submission containing user response and timing information</param>
    /// <param name="cancellationToken">Cancellation token for request cancellation</param>
    /// <returns>Answer evaluation result with score</returns>
    /// <remarks>
    /// Submits and evaluates a user's answer to a specific quiz question.
    /// The system calculates points, updates the user's total score and leaderboard position.
    /// </remarks>
    /// <response code="200">Answer submitted and evaluated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Quiz or question not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("answer")]
    [ProducesResponseType(typeof(SubmissionResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<SubmissionResult>> SubmitAnswer(
        [FromBody] SubmitAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("User {UserId} submitting answer for question {QuestionId} in quiz {QuizId}",
                _userContext.GetCurrentUser().UserId, request.QuestionId, request.QuizId);

            var result = await _quizService.SubmitAnswerAsync(_userContext.GetCurrentUser().UserId, request, cancellationToken);

            _logger.LogInformation("Answer submitted successfully. User {UserId}, Question {QuestionId}, Correct: {IsCorrect}, Points: {Points}",
                _userContext.GetCurrentUser().UserId, request.QuestionId, result.IsCorrect, result.PointsEarned);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid answer submission: {Message}", ex.Message);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid Answer Submission",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Quiz or question not found: {Message}", ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Quiz or Question Not Found",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Answer submission cancelled");
            return StatusCode((int)HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit answer for user {UserId}, question {QuestionId}",
                _userContext.GetCurrentUser().UserId, request.QuestionId);
            return StatusCode((int)HttpStatusCode.InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while submitting the answer",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }
}

/// <summary>
/// Request model for joining a quiz
/// </summary>
public class JoinQuizRequest
{
    /// <summary>
    /// Unique identifier for the quiz to join (required)
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [Required(ErrorMessage = "Quiz ID is required")]
    public Guid QuizId { get; set; }
}

/// <summary>
/// Response model for joining a quiz
/// </summary>
public class JoinQuizResponse
{
    /// <summary>
    /// Quiz identifier
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid QuizId { get; set; }

    /// <summary>
    /// User quiz session identifier
    /// </summary>
    /// <example>1651c284-3a36-4870-b1ea-9720cc95c0cb</example>
    public Guid UserQuizId { get; set; }

    /// <summary>
    /// Quiz title
    /// </summary>
    /// <example>English Vocabulary Quiz</example>
    public string Title { get; set; }

    /// <summary>
    /// Quiz description
    /// </summary>
    /// <example>Test your English vocabulary knowledge</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Total time limit for the quiz
    /// </summary>
    /// <example>00:30:00</example>
    public TimeSpan TimeLimit { get; set; }

}

/// <summary>
/// Request model for submitting an answer
/// </summary>
public class SubmitAnswerRequest
{
    /// <summary>
    /// Quiz identifier (required)
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [Required(ErrorMessage = "Quiz ID is required")]
    public Guid QuizId { get; set; }

    /// <summary>
    /// Question identifier (required)
    /// </summary>
    /// <example>456e7890-e12b-34c5-d678-901234567890</example>
    [Required(ErrorMessage = "Question ID is required")]
    public Guid QuestionId { get; set; }

    /// <summary>
    /// User's answer (required, 1-500 characters)
    /// </summary>
    /// <example>Present everywhere</example>
    [Required(ErrorMessage = "Answer is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Answer must be between 1 and 500 characters")]
    public string Answer { get; set; } = string.Empty;
}

/// <summary>
/// Response model for answer submission
/// </summary>
public class SubmissionResult
{
    /// <summary>
    /// User session identifier
    /// </summary>
    /// <example>456e7890-e12b-34c5-d678-901234567890</example>
    public Guid UserQuizId { get; set; }

    /// <summary>
    /// Whether the answer was correct
    /// </summary>
    /// <example>true</example>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Points earned for this answer
    /// </summary>
    /// <example>10</example>
    public int PointsEarned { get; set; }

    /// <summary>
    /// Total points earned
    /// </summary>
    /// <example>100</example>
    public int TotalPointsEarned { get; set; }
}