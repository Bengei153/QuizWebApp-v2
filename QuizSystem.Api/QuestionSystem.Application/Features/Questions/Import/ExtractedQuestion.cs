namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Import;

/// <summary>
/// A single question as parsed by the AI from raw pasted text. This is a
/// transport shape only — it gets converted into real CreateQuestionCommand /
/// AddOptionCommand calls by ImportQuestionsHandler, so all your existing
/// domain validation and ownership checks still run exactly as before.
/// </summary>
public sealed class ExtractedQuestion
{
    public string Text { get; init; } = "";

    /// <summary>
    /// Must match one of your QuestionType enum names exactly:
    /// "Text", "SingleChoice", "MultipleChoice", "TrueFalse".
    /// Kept as a string here (not the enum) because it's coming from JSON the
    /// AI produced — we validate/parse it ourselves in the handler rather
    /// than trusting System.Text.Json to silently fail on a bad value.
    /// </summary>
    public string QuestionType { get; init; } = "";

    public List<ExtractedOption> Options { get; init; } = new();
}

public sealed class ExtractedOption
{
    public string Text { get; init; } = "";
    public bool IsCorrect { get; init; }
}

/// <summary>
/// Top-level shape the AI returns for a whole pasted block of questions.
/// </summary>
public sealed class ExtractedQuestionSet
{
    public List<ExtractedQuestion> Questions { get; init; } = new();
}