using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestions.GetAllQuestions;

public sealed class GetAllQuestionsCommand
{
    public GetAllQuestionsCommand(Guid folderId) => FolderId = folderId;

    public Guid FolderId { get; init; }
}