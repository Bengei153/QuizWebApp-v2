using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.DeleteQuestion;

public sealed class DeleteQuestionHandler
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteQuestionHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteQuestionCommand command)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Couldn't get user");

        var question = await _questionRepository.GetByIdAsync(command.QuestionId, command.FolderId);
        if (question is null)
            throw new InvalidOperationException("Question not found in this folder");

        if (question.CreatedByUserId != userId && !_currentUserService.IsAdmin)
            throw new ForbiddenAccessException("You do not have permission to delete this question");

        question.SoftDelete();

        await _unitOfWork.SaveChangesAsync();
    }
}
