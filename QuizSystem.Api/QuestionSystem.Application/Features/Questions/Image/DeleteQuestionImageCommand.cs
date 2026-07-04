using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Image;

public sealed class DeleteQuestionImageCommand
{
    public DeleteQuestionImageCommand(Guid questionId) => QuestionId = questionId;

    public Guid QuestionId { get; init; }
}
