using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestions.GetAllQuestions;
using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestion.GetAllQuestionsHandler;

public sealed class GetAllQuestionsHandler
{
    private readonly IQuestionRepository _questionRepository;

    public GetAllQuestionsHandler(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public async Task<List<QuestionDto>> Handle(GetAllQuestionsCommand command)
    {
        var questions = await _questionRepository.GetAllByFolderIdAsync(command.FolderId);

        return questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            Text = q.Text,
            FolderId = q.FolderId,
            Type = q.Type,
            ImageUrl = q.ImageUrl,
            Options = q.Options
                .Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    ImageUrl = o.ImageUrl
                })
                .ToList()
        }).ToList();
    }
}