using System;
using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class SubmitQuizHandler : IRequestHandler<SubmitQuizCommand, QuizResultDto>
{
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitQuizHandler(
        IQuizAttemptRepository attemptRepository,
        IFolderRepository folderRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _attemptRepository = attemptRepository;
        _folderRepository = folderRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<QuizResultDto> Handle(SubmitQuizCommand command, CancellationToken cancellationToken)
    {
        var attempt = await _attemptRepository.GetWithAnswersAsync(command.AttemptId);
        if (attempt == null)
            throw new InvalidOperationException("Attempt not found.");

        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            throw new UnauthorizedAccessException("User not logged in");

        var userId = Guid.Parse(_currentUser.UserId);
        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException("Not your attempt.");

        if (attempt.isCompleted)
            throw new InvalidOperationException("Quiz already submitted.");

        // QuizAttempt.QuestionGroupId actually stores the FOLDER's Id (see
        // StartQuizHandler) — a pre-existing naming mismatch, not something
        // new. Using GetByIdsAsync since it doesn't require the real group
        // id, which was never stored on the attempt.
        var folders = await _folderRepository.GetByIdsAsync(new List<Guid> { attempt.QuestionGroupId });
        var folder = folders.FirstOrDefault();
        if (folder == null)
            throw new InvalidOperationException("Quiz folder no longer exists.");

        var results = new List<GradedQuestionDto>();
        var earnedPoints = 0;
        var questions = folder.Questions.Where(q => !q.IsDeleted).ToList();
        var totalPoints = questions.Count;

        foreach (var question in questions)
        {
            var correctOptionIds = question.Options.Where(o => o.isCorrect).Select(o => o.Id).ToList();

            // Only the first selected option is graded for now — the schema
            // (AttemptAnswer.SelectedOptionId is a single Guid) doesn't yet
            // support multiple selections per question. Real multi-select
            // grading needs a schema change; flagged as a known follow-up.
            var userSelectedIds = command.Answers.TryGetValue(question.Id, out var selected) && selected.Any()
                ? new List<Guid> { selected.First() }
                : new List<Guid>();

            var isCorrect = userSelectedIds.Count == correctOptionIds.Count &&
                             correctOptionIds.All(id => userSelectedIds.Contains(id));

            if (isCorrect)
                earnedPoints++;

            attempt.Answers.Add(new Domain.Entities.AttemptAnswer
            {
                Id = Guid.NewGuid(),
                QuizAttemptId = attempt.Id,
                QuestionId = question.Id,
                SelectedOptionId = userSelectedIds.FirstOrDefault(),
                IsCorrect = isCorrect
            });

            results.Add(new GradedQuestionDto
            {
                Id = question.Id,
                Text = question.Text.Value,
                Options = question.Options.Select(o => new OptionResultDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.isCorrect
                }).ToList(),
                UserSelected = userSelectedIds,
                CorrectOptions = correctOptionIds
            });
        }

        attempt.Score = earnedPoints;
        attempt.TotalQuestions = totalPoints;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.isCompleted = true;

        await _attemptRepository.UpdateAsync(attempt);
        await _unitOfWork.SaveChangesAsync();

        var scorePercentage = totalPoints > 0 ? (int)Math.Round((double)earnedPoints / totalPoints * 100) : 0;

        return new QuizResultDto
        {
            Score = scorePercentage,
            EarnedPoints = earnedPoints,
            TotalPoints = totalPoints,
            Results = results
        };
    }
}