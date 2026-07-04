using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Options;

public sealed class RemoveOptionCommand
{
    public RemoveOptionCommand(Guid questionId, Guid optionId)
    {
        QuestionId = questionId;
        OptionId = optionId;
    }

    public Guid QuestionId { get; init; }
    public Guid OptionId { get; init; }
}
