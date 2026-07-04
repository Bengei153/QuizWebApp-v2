namespace QuizSystem.Api.QuestionSystem.Infrastructure.Services;

public sealed class AnthropicSettings
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Cheapest model, plenty capable for structured extraction. Swap to a
    /// larger model here (one config value) if you ever find Haiku
    /// unreliable on messier input — no other code needs to change.
    /// </summary>
    public string Model { get; set; } = "claude-haiku-4-5-20251001";
}