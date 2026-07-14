using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class GetStudentFoldersHandler : IRequestHandler<GetStudentFoldersQuery, List<StudentFolderDto>>
{
    private readonly IQuestionGroupRepository _groupRepository;
    private readonly ICurrentUserService _currentUser;

    public GetStudentFoldersHandler(IQuestionGroupRepository groupRepository, ICurrentUserService currentUser)
    {
        _groupRepository = groupRepository;
        _currentUser = currentUser;
    }

    public async Task<List<StudentFolderDto>> Handle(
        GetStudentFoldersQuery request,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId;
        if (string.IsNullOrWhiteSpace(orgId))
            throw new UnauthorizedAccessException("User must belong to an organisation to view quizzes.");

        var groups = await _groupRepository.GetAllByOrganisationAsync(orgId);

        return groups
            .Where(g => !g.IsDeleted)
            .SelectMany(g => g.Folders
                .Where(f => !f.IsDeleted)
                .Select(f => new StudentFolderDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    GroupId = g.Id,
                    GroupName = g.Name,
                    QuestionCount = f.Questions.Count(q => !q.IsDeleted)
                }))
            .Where(f => f.QuestionCount > 0)
            .OrderBy(f => f.GroupName)
            .ThenBy(f => f.Name)
            .ToList();
    }
}