using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.AI;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.CreateQuestion;
using QuizSystem.Api.QuestionSystem.Domain.Enums;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.Import;

public sealed class ImportQuestionsHandler
{
    private readonly IQuizGenerationService _quizGenerationService;
    private readonly CreateQuestionHandler _createQuestionHandler;
    private readonly AddOptionHandler _addOptionHandler;

    public ImportQuestionsHandler(
        IQuizGenerationService quizGenerationService,
        CreateQuestionHandler createQuestionHandler,
        AddOptionHandler addOptionHandler)
    {
        _quizGenerationService = quizGenerationService;
        _createQuestionHandler = createQuestionHandler;
        _addOptionHandler = addOptionHandler;
    }

    public async Task<ImportQuestionsResult> Handle(ImportQuestionsCommand command, CancellationToken ct = default)
    {
        var extracted = await _quizGenerationService.ExtractQuestionsAsync(command.RawText, ct);
        var result = new ImportQuestionsResult();

        foreach (var q in extracted.Questions)
        {
            if (!Enum.TryParse<QuestionType>(q.QuestionType, ignoreCase: true, out var type))
            {
                result.Errors.Add($"Skipped \"{q.Text}\": unrecognized question type \"{q.QuestionType}\"");
                continue;
            }

            Guid questionId;
            try
            {
                var createCommand = new CreateQuestionCommand(q.Text, type, command.FolderId, command.GroupId);
                questionId = await _createQuestionHandler.Handle(createCommand);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to create \"{q.Text}\": {ex.Message}");
                continue;
            }

            foreach (var opt in q.Options)
            {
                try
                {
                    await _addOptionHandler.Handle(new AddOptionCommand
                    {
                        QuestionId = questionId,
                        Text = opt.Text,
                        IsCorrect = opt.IsCorrect
                    });
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"\"{q.Text}\" created, but option \"{opt.Text}\" failed: {ex.Message}");
                }
            }

            result.CreatedQuestionIds.Add(questionId);
        }

        return result;
    }
}