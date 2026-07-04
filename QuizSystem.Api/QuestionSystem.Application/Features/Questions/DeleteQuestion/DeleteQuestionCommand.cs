using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.DeleteQuestion;

public sealed class DeleteQuestionCommand
{
    public DeleteQuestionCommand(Guid questionId, Guid folderId)
    {
        QuestionId = questionId;
        FolderId = folderId;
    }

    public Guid QuestionId { get; init; }
    public Guid FolderId { get; init; }
}
