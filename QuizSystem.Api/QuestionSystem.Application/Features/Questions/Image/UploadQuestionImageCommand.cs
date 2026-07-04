using System;
using QuizSystem.Api.QuestionSystem.Application.Features.Images;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Image;

public sealed class UploadQuestionImageCommand
{
    public Guid QuestionId { get; init; }
    public required ImageUploadRequest File { get; init; }
}
