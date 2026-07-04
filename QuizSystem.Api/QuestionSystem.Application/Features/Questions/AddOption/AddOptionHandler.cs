using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;

public sealed class AddOptionHandler
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddOptionHandler(
        IQuestionRepository questionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <summary>Returns the newly created option's Id.</summary>
    public async Task<Guid> Handle(AddOptionCommand command)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Couldn't get user");

        var question = await _questionRepository
            .GetWithOptionsAsync(command.QuestionId);

        if (question is null)
            throw new InvalidOperationException("Question not found");

        if (question.CreatedByUserId != userId && !_currentUserService.IsAdmin)
            throw new ForbiddenAccessException("You do not have permission to modify this question");

        var option = question.AddOption(command.Text, command.IsCorrect);

        await _unitOfWork.SaveChangesAsync();

        return option.Id;
    }
}
