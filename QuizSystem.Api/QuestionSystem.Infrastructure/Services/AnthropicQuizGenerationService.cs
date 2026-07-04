using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.AI;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.Import;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Services;

public sealed class AnthropicQuizGenerationService : IQuizGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicSettings _settings;

    public AnthropicQuizGenerationService(HttpClient httpClient, IOptions<AnthropicSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException(
                "Anthropic API key is not configured. Set Anthropic:ApiKey via user-secrets or an environment variable.");
        }
    }

    private const string SystemPrompt = """
        You extract quiz questions from raw, messy, user-pasted text and convert
        them into structured JSON.

        Output ONLY valid JSON matching this exact shape. No explanation, no
        markdown code fences, no commentary before or after the JSON.

        {
          "questions": [
            {
              "text": "the question text",
              "questionType": "SingleChoice",
              "options": [
                { "text": "option text", "isCorrect": true }
              ]
            }
          ]
        }

        Rules:
        - "questionType" must be exactly one of: "Text", "SingleChoice",
          "MultipleChoice", "TrueFalse". Infer it from context: a question
          with numbered/lettered choices and one right answer is
          "SingleChoice"; multiple correct answers implied is
          "MultipleChoice"; yes/no or true/false is "TrueFalse"; free-response
          with no listed choices is "Text".
        - For "Text" type questions, "options" must be an empty array [].
        - If the source text doesn't clearly mark which option is correct,
          set every option's "isCorrect" to false rather than guessing.
        - Preserve the original question wording; fix obvious typos in
          option text only (e.g. clear misspellings of real answers).

        Example input:
        What is the capital of Nigeria.
        1) Abuja
        2) Lagos
        3) Kano
        4) Kastina

        Example output:
        {
          "questions": [
            {
              "text": "What is the capital of Nigeria.",
              "questionType": "SingleChoice",
              "options": [
                { "text": "Abuja", "isCorrect": true },
                { "text": "Lagos", "isCorrect": false },
                { "text": "Kano", "isCorrect": false },
                { "text": "Katsina", "isCorrect": false }
              ]
            }
          ]
        }
        """;

    public async Task<ExtractedQuestionSet> ExtractQuestionsAsync(
        string rawText,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = _settings.Model,
            max_tokens = 4000,
            temperature = 0,
            system = SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = rawText }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _settings.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new DomainException(
                $"AI question extraction failed (HTTP {(int)response.StatusCode}): {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var replyText = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "";

        // Defensive: strip markdown fences if the model added them anyway,
        // despite the system prompt telling it not to. Don't trust the
        // model blindly — this is the cheap insurance from Stage 5.
        replyText = replyText.Trim();
        if (replyText.StartsWith("```"))
        {
            var firstNewline = replyText.IndexOf('\n');
            var lastFence = replyText.LastIndexOf("```");
            if (firstNewline > 0 && lastFence > firstNewline)
                replyText = replyText[(firstNewline + 1)..lastFence].Trim();
        }

        try
        {
            var result = JsonSerializer.Deserialize<ExtractedQuestionSet>(
                replyText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new ExtractedQuestionSet();
        }
        catch (JsonException ex)
        {
            throw new DomainException(
                $"AI returned invalid JSON that could not be parsed: {ex.Message}. Raw reply: {replyText}");
        }
    }
}