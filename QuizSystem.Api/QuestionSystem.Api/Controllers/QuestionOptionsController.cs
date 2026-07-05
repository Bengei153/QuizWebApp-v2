using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Features.Images;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.Options;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers;

/// <summary>
/// Manages options belonging to a question, including per-option images.
/// Ownership is enforced inside the handlers (the option's parent question's
/// CreatedByUserId must match the current user, or the user must be Admin).
/// </summary>
[Route("api/questions/{questionId:guid}/options")]
[ApiController]
[Authorize]
public sealed class QuestionOptionsController : ControllerBase
{
    private readonly AddOptionHandler _addHandler;
    private readonly RemoveOptionHandler _removeHandler;
    private readonly UploadOptionImageHandler _uploadImageHandler;
    private readonly DeleteOptionImageHandler _deleteImageHandler;

    public QuestionOptionsController(
        AddOptionHandler addHandler,
        RemoveOptionHandler removeHandler,
        UploadOptionImageHandler uploadImageHandler,
        DeleteOptionImageHandler deleteImageHandler)
    {
        _addHandler = addHandler;
        _removeHandler = removeHandler;
        _uploadImageHandler = uploadImageHandler;
        _deleteImageHandler = deleteImageHandler;
    }

    /// <summary>
    /// Add an option to a question. User must own the question OR be Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin, OrgAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Add(Guid questionId, [FromBody] AddOptionRequestDto request)
    {
        var optionId = await _addHandler.Handle(new AddOptionCommand
        {
            QuestionId = questionId,
            Text = request.Text,
            IsCorrect = request.IsCorrect
        });

        return StatusCode(StatusCodes.Status201Created, new { id = optionId });
    }

    /// <summary>
    /// Remove an option (soft delete). User must own the question OR be Admin.
    /// </summary>
    [HttpDelete("{optionId:guid}")]
    [Authorize(Roles = "OrgAdmin, SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveOption(Guid questionId, Guid optionId)
    {
        await _removeHandler.Handle(new RemoveOptionCommand(questionId, optionId));
        return NoContent();
    }

    /// <summary>
    /// Upload/replace an option's image. Enforces that this image's aspect
    /// ratio matches any other image already attached to a sibling option on
    /// the same question — returns 400 if it doesn't.
    /// </summary>
    [HttpPost("{optionId:guid}/image")]
    [Authorize(Roles = "OrgAdmin, SuperAdmin")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadOptionImage(Guid questionId, Guid optionId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        await using var stream = file.OpenReadStream();

        var url = await _uploadImageHandler.Handle(new UploadOptionImageCommand
        {
            QuestionId = questionId,
            OptionId = optionId,
            File = new ImageUploadRequest
            {
                Content = stream,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length
            }
        });

        return Ok(new { imageUrl = url });
    }

    /// <summary>
    /// Remove an option's image.
    /// </summary>
    [HttpDelete("{optionId:guid}/image")]
    [Authorize(Roles = "OrgAdmin, SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOptionImage(Guid questionId, Guid optionId)
    {
        await _deleteImageHandler.Handle(new DeleteOptionImageCommand(questionId, optionId));
        return NoContent();
    }
}

public sealed class AddOptionRequestDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
