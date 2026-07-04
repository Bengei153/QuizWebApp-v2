using System;
using QuizSystem.Api.QuestionSystem.Application.Features.Images;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Options;

public sealed class UploadOptionImageCommand
{
    public Guid QuestionId { get; init; }
    public Guid OptionId { get; init; }
    public required ImageUploadRequest File { get; init; }
}
