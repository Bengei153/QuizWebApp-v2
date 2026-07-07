using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup
{
    public class GetMyQuestionGroupsHandler
    {
        private readonly IQuestionGroupRepository _repository;

        public GetMyQuestionGroupsHandler(IQuestionGroupRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<QuestionGroupDto>> Handle(GetMyQuestionGroupsCommand command)
        {
            var orgId = command.User.OrganisationId;
            if (string.IsNullOrWhiteSpace(orgId))
                throw new ForbiddenAccessException("User/Organisation identification failed");

            var groups = await _repository.GetAllByOrganisationAsync(orgId);

            return groups.Select(g => new QuestionGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                CreatedByUserId = g.CreatedByUserId,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt,
                IsDeleted = g.IsDeleted
            }).ToList();
        }
    }
}