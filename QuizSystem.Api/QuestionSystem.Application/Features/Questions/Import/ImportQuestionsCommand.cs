using System;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.CreateQuestion;
using QuizSystem.Api.QuestionSystem.Domain.Enums;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Import;

public sealed class ImportQuestionsCommand
{
    public required string RawText { get; init; }
    public Guid FolderId { get; init; }
    public Guid GroupId { get; init; }
}

public sealed class ImportQuestionsResult
{
    public List<Guid> CreatedQuestionIds { get; init; } = new();
    public List<string> Errors { get; init; } = new();
}