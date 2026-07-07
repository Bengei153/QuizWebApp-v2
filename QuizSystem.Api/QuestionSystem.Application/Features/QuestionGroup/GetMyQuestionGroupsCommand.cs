using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;

public sealed class GetMyQuestionGroupsCommand
{
    public CurrentUserContext User { get; }
    public GetMyQuestionGroupsCommand(CurrentUserContext user) => User = user;
}
