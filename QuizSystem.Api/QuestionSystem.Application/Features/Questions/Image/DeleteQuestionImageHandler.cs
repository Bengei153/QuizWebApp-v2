using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Storage;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Image;

public sealed class DeleteQuestionImageHandler
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IImageStorageService _imageStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteQuestionImageHandler(
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

    public async Task Handle(DeleteQuestionImageCommand command, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Couldn't get user");

        var question = await _questionRepository.GetWithOptionsAsync(command.QuestionId);
        if (question is null)
            throw new InvalidOperationException("Question not found");

        if (question.CreatedByUserId != userId && !_currentUserService.IsAdmin)
            throw new ForbiddenAccessException("You do not have permission to modify this question");

        var publicId = question.ImagePublicId;
        question.ClearImage();

        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(publicId))
        {
            try { await _imageStorage.DeleteAsync(publicId, ct); }
            catch { /* asset cleanup is non-critical */ }
        }
    }
}
