using QuizSystem.Api.QuestionSystem.Application.Features.Questions.Import;

namespace QuizSystem.Api.QuestionSystem.Application.Abstractions.AI;

/// <summary>
/// Abstraction over whichever AI provider turns raw pasted quiz text into
/// structured questions. Swapping Claude for another provider (or a local
/// model) later means writing one new implementation of this interface —
/// nothing in Application or Domain changes. Same reasoning as
/// IImageStorageService.
/// </summary>
public interface IQuizGenerationService
{
    Task<ExtractedQuestionSet> ExtractQuestionsAsync(
        string rawText,
        CancellationToken cancellationToken = default);
}