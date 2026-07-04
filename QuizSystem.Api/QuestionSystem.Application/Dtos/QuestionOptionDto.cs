namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public sealed class QuestionOptionDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = null!;
    public string? ImageUrl { get; init; }
}
