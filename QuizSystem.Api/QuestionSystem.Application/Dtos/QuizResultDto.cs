namespace QuizSystem.Api.QuestionSystem.Application;

public class QuizResultDto
{
    public int Score { get; set; }
    public int EarnedPoints { get; set; }
    public int TotalPoints { get; set; }
    public List<QuestionResultDto> Results { get; set; } = new();
}

public class QuestionResultDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = "";
    public List<OptionResultDto> Options { get; set; } = new();
    public List<Guid> UserSelected { get; set; } = new();
    public List<Guid> CorrectOptions { get; set; } = new();
}

public class OptionResultDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
}

public class SubmitQuizRequestDto
{
    public Dictionary<Guid, List<Guid>> Answers { get; set; } = new();
}