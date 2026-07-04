using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Storage;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Options;

public sealed class DeleteOptionImageHandler
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IImageStorageService _imageStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteOptionImageHandler(
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

    public async Task Handle(DeleteOptionImageCommand command, CancellationToken ct = default)
    {
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

        var publicId = option.ImagePublicId;
        option.ClearImage();

        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(publicId))
        {
            try { await _imageStorage.DeleteAsync(publicId, ct); }
            catch { /* asset cleanup is non-critical */ }
        }
    }
}
