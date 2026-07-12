using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestions;

public sealed class GetAllQuestionsCommand
{
    public GetAllQuestionsCommand(Guid folderId) => FolderId = folderId;

    public Guid FolderId { get; init; }
}