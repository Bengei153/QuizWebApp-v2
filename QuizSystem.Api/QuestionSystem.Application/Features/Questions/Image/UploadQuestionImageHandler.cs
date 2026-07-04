using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Storage;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Image;

public sealed class UploadQuestionImageHandler
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IImageStorageService _imageStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UploadQuestionImageHandler(
        IQuestionRepository questionRepository,
        IImageStorageService imageStorage,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _imageStorage = imageStorage;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(UploadQuestionImageCommand command, CancellationToken ct = default)
    {
        command.File.Validate();

        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Couldn't get user");

        var question = await _questionRepository.GetWithOptionsAsync(command.QuestionId);
        if (question is null)
            throw new InvalidOperationException("Question not found");

        if (question.CreatedByUserId != userId && !_currentUserService.IsAdmin)
            throw new ForbiddenAccessException("You do not have permission to modify this question");

        var previousPublicId = question.ImagePublicId;

        var result = await _imageStorage.UploadAsync(
            command.File.Content,
            command.File.FileName,
            command.File.ContentType,
            ct);

        question.SetImage(result.Url, result.PublicId);

        await _unitOfWork.SaveChangesAsync();

        // Best-effort cleanup of the old asset; failing here shouldn't fail the
        // request since the new image is already saved and live.
        if (!string.IsNullOrWhiteSpace(previousPublicId))
        {
            try { await _imageStorage.DeleteAsync(previousPublicId, ct); }
            catch { /* old asset cleanup is non-critical */ }
        }

        return result.Url;
    }
}
