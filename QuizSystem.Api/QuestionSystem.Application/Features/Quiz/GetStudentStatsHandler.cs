using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class GetStudentStatsHandler : IRequestHandler<GetStudentStatsQuery, StudentStatsDto>
{
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentStatsHandler(
        IQuizAttemptRepository attemptRepository,
        IQuestionRepository questionRepository,
        IFolderRepository folderRepository,
        ICurrentUserService currentUserService)
    {
        _attemptRepository = attemptRepository;
        _questionRepository = questionRepository;
        _folderRepository = folderRepository;
        _currentUserService = currentUserService;
    }

    public async Task<StudentStatsDto> Handle(
        GetStudentStatsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            throw new UnauthorizedAccessException("User must be authenticated");

        var userId = Guid.Parse(_currentUserService.UserId);
        var attempts = await _attemptRepository.GetByUserWithAnswersAsync(userId);

        var completedAttempts = attempts.Where(a => a.isCompleted).ToList();
        var totalQuestions = completedAttempts.Sum(a => a.TotalQuestions);
        var correctAnswers = completedAttempts.Sum(a => a.Score);
        var totalAnswers = totalQuestions;
        var inProgressAttempts = attempts.Count(a => !a.isCompleted);

        var averageScore = completedAttempts.Any()
            ? completedAttempts.Average(a => a.Score)
            : 0;

        var passRate = totalAnswers > 0
            ? (double)correctAnswers / totalAnswers * 100
            : 0;

        var suggestedTopics = await BuildSuggestedTopicsAsync(completedAttempts);

        return new StudentStatsDto
        {
            TotalQuizzesTaken = completedAttempts.Count,
            AverageScore = Math.Round(averageScore, 2),
            CorrectAnswers = correctAnswers,
            TotalAnswers = totalAnswers,
            PassRate = Math.Round(passRate, 2),
            QuizzesInProgress = inProgressAttempts,
            SuggestedTopics = suggestedTopics
        };
    }

    /// <summary>
    /// Ranks folders by the student's accuracy on questions from that folder,
    /// worst first, and returns the 3 weakest as "suggested topics." Folders
    /// with fewer than 2 answered questions are excluded — one lucky or
    /// unlucky guess shouldn't brand a whole topic as a weak point.
    /// </summary>
    private async Task<List<SuggestedTopicDto>> BuildSuggestedTopicsAsync(
        List<Domain.Entities.QuizAttempt> completedAttempts)
    {
        var allAnswers = completedAttempts.SelectMany(a => a.Answers).ToList();
        if (!allAnswers.Any())
            return new List<SuggestedTopicDto>();

        var questionIds = allAnswers.Select(a => a.QuestionId).Distinct().ToList();
        var questions = await _questionRepository.GetByIdsAsync(questionIds);
        var questionToFolder = questions.ToDictionary(q => q.Id, q => q.FolderId);

        var folderStats = allAnswers
            .Where(a => questionToFolder.ContainsKey(a.QuestionId))
            .GroupBy(a => questionToFolder[a.QuestionId])
            .Select(g => new
            {
                FolderId = g.Key,
                Total = g.Count(),
                Correct = g.Count(a => a.IsCorrect)
            })
            .Where(g => g.Total >= 2)
            .OrderBy(g => (double)g.Correct / g.Total)
            .Take(3)
            .ToList();

        if (!folderStats.Any())
            return new List<SuggestedTopicDto>();

        var folders = await _folderRepository.GetByIdsAsync(folderStats.Select(f => f.FolderId).ToList());
        var folderLookup = folders.ToDictionary(f => f.Id);

        return folderStats
            .Where(f => folderLookup.ContainsKey(f.FolderId))
            .Select(f =>
            {
                var folder = folderLookup[f.FolderId];
                var accuracy = Math.Round((double)f.Correct / f.Total * 100, 0);
                return new SuggestedTopicDto
                {
                    Id = folder.Id,
                    Tag = "WEAK POINT",
                    Title = folder.Name,
                    Desc = $"{accuracy}% accuracy across {f.Total} question{(f.Total == 1 ? "" : "s")}",
                    GroupId = folder.QuestionGroupId
                };
            })
            .ToList();
    }
}