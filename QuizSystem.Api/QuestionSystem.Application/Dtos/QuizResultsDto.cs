namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class QuizResultsDto
{
    public Guid AttemptId { get; set; }
    public string GroupName { get; set; } = null!;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage { get; set; }
    public bool Passed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<QuestionResultDto> Questions { get; set; } = new();
}

public class QuestionResultDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public string? SelectedAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
}


/*public class QuestionResultDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = "";
    public List<OptionResultDto> Options { get; set; } = new();
    public List<Guid> UserSelected { get; set; } = new();
    public List<Guid> CorrectOptions { get; set; } = new();
}*/
