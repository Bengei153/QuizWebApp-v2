using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application;
using QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers
{
    [Route("api/quizzes")]
    [ApiController]
    [Authorize]
    public class QuizAttemptController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuizAttemptController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartQuiz(Guid id, [FromQuery] Guid groupId)
        {
            try
            {
                var result = await _mediator.Send(new StartQuizCommand(id, groupId));
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitQuizRequestDto request)
        {
            try
            {
                var result = await _mediator.Send(new SubmitQuizCommand(id, request.Answers));
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(Guid id)
        {
            try
            {
                var results = await _mediator.Send(new GetQuizResultsQuery(id));
                return Ok(results);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet("my-attempts")]
        public async Task<IActionResult> GetMyAttempts(Guid attemptId)
        {
            try
            {
                var attempts = await _mediator.Send(new GetAttemptDetailsQuery(attemptId));
                return Ok(attempts);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet("{attemptId}")]
        public async Task<IActionResult> GetAttempt(Guid attemptId)
        {
            try
            {
                var attempt = await _mediator.Send(new GetAttemptDetailsQuery(attemptId));
                return Ok(attempt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }
    }
}
