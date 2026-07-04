using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Features.Images;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.CreateQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.DeleteQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.Image;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.Import;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.UpdateQuestion;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Domain.Enums;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers;

/// <summary>
/// Question API Controller. Ownership is enforced inside the handlers
/// (CreatedByUserId == current user, or Admin), consistent with
/// SecuredFolderController.
/// </summary>
[Route("api/folders/{folderId:guid}/questions")]
[ApiController]
[Authorize]
public class SecuredQuestionController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly CreateQuestionHandler _createHandler;
    private readonly GetQuestionHandler _getHandler;
    private readonly UpdateQuestionHandler _updateHandler;
    private readonly DeleteQuestionHandler _deleteHandler;
    private readonly UploadQuestionImageHandler _uploadImageHandler;
    private readonly DeleteQuestionImageHandler _deleteImageHandler;

    public SecuredQuestionController(
        ICurrentUserService currentUserService,
        CreateQuestionHandler createHandler,
        GetQuestionHandler getHandler,
        UpdateQuestionHandler updateHandler,
        DeleteQuestionHandler deleteHandler,
        UploadQuestionImageHandler uploadImageHandler,
        DeleteQuestionImageHandler deleteImageHandler)
    {
        _currentUserService = currentUserService;
        _createHandler = createHandler;
        _getHandler = getHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _uploadImageHandler = uploadImageHandler;
        _deleteImageHandler = deleteImageHandler;
    }

    /// <summary>
    /// Create a question. Only Creator and Admin roles can create.
    /// Requires a groupId because folders are scoped to a question group.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateQuestion(
        Guid folderId,
        [FromBody] CreateQuestionRequestDto request)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "User identification failed" });

        var command = new CreateQuestionCommand(request.Text, request.Type, folderId, request.GroupId);
        await _createHandler.Handle(command);

        var questionId = await _createHandler.Handle(command);

        return StatusCode(StatusCodes.Status201Created, new { id = questionId, message = "Question created successfully" });
    }

    /// <summary>
    /// Get a question. All authenticated users (any role) can read.
    /// </summary>
    [HttpGet("{questionId:guid}")]
    [Authorize(Roles = "Admin, Creator, Viewer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestion(Guid folderId, Guid questionId)
    {
        var result = await _getHandler.Handle(new GetQuestionQuery { QuestionId = questionId });

        if (result is null || result.FolderId != folderId)
            return NotFound(new { message = "Question not found in this folder" });

        return Ok(result);
    }

    /// <summary>
    /// Update a question. User must own it OR be Admin.
    /// </summary>
    [HttpPut("{questionId:guid}")]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuestion(
        Guid folderId,
        Guid questionId,
        [FromBody] UpdateQuestionRequestDto request)
    {
        var command = new UpdateQuestionCommand(request.Text, request.Type, folderId, request.GroupId)
        {
            Id = questionId
        };

        var result = await _updateHandler.Handle(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete a question (soft delete). User must own it OR be Admin.
    /// </summary>
    [HttpDelete("{questionId:guid}")]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(Guid folderId, Guid questionId)
    {
        await _deleteHandler.Handle(new DeleteQuestionCommand(questionId, folderId));
        return NoContent();
    }

    /// <summary>
    /// Upload/replace this question's image. User must own it OR be Admin.
    /// Max 5MB; jpeg/png/webp/gif only (enforced in ImageUploadRequest.Validate()).
    /// </summary>
    [HttpPost("{questionId:guid}/image")]
    [Authorize(Roles = "Admin, Creator")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadQuestionImage(Guid folderId, Guid questionId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        await using var stream = file.OpenReadStream();

        var url = await _uploadImageHandler.Handle(new UploadQuestionImageCommand
        {
            QuestionId = questionId,
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
    /// Remove this question's image. User must own it OR be Admin.
    /// </summary>
    [HttpDelete("{questionId:guid}/image")]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestionImage(Guid folderId, Guid questionId)
    {
        await _deleteImageHandler.Handle(new DeleteQuestionImageCommand(questionId));
        return NoContent();
    }

    /// <summary>
    /// Paste raw quiz text (any messy format) and have the AI extract and
    /// create the questions/options automatically. Requires GroupId since
    /// folders are scoped to a question group, same as manual creation.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportQuestions(
        Guid folderId,
        [FromBody] ImportQuestionsRequestDto request,
        [FromServices] ImportQuestionsHandler importHandler)
    {
        if (string.IsNullOrWhiteSpace(request.RawText))
            return BadRequest(new { message = "No text provided to import" });

        var result = await importHandler.Handle(new ImportQuestionsCommand
        {
            RawText = request.RawText,
            FolderId = folderId,
            GroupId = request.GroupId
        });

        return Ok(new
        {
            createdCount = result.CreatedQuestionIds.Count,
            createdQuestionIds = result.CreatedQuestionIds,
            errors = result.Errors
        });
    }
}

public class CreateQuestionRequestDto
{
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public Guid GroupId { get; set; }
}

public class UpdateQuestionRequestDto
{
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public Guid GroupId { get; set; }
}
