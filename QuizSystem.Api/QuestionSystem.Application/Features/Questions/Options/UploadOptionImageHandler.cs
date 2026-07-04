using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Storage;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Options;

public sealed class UploadOptionImageHandler
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IImageStorageService _imageStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UploadOptionImageHandler(
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

    public async Task<string> Handle(UploadOptionImageCommand command, CancellationToken ct = default)
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

        var option = question.Options.FirstOrDefault(o => o.Id == command.OptionId);
        if (option is null)
            throw new InvalidOperationException("Option not found on this question");

        // Upload first so we know the real dimensions, THEN validate the
        // aspect-ratio rule before committing it to the option. This avoids
        // silently accepting a mismatched image if validation happened only
        // after the fact.
        var result = await _imageStorage.UploadAsync(
            command.File.Content,
            command.File.FileName,
            command.File.ContentType,
            ct);

        try
        {
            question.EnsureOptionImageAspectRatioIsConsistent(result.Width, result.Height);
        }
        catch
        {
            // Reject: clean up the asset we just uploaded so it doesn't
            // become an orphaned file in storage, then re-throw the original
            // DomainException so the client sees the real 400 message.
            try { await _imageStorage.DeleteAsync(result.PublicId, ct); } catch { /* best-effort */ }
            throw;
        }

        var previousPublicId = option.ImagePublicId;
        option.SetImage(result.Url, result.PublicId, result.Width, result.Height);

        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(previousPublicId))
        {
            try { await _imageStorage.DeleteAsync(previousPublicId, ct); }
            catch { /* old asset cleanup is non-critical */ }
        }

        return result.Url;
    }
}
