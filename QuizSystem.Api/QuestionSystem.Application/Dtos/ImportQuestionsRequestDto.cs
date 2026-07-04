namespace QuizSystem.Api.QuestionSystem.Application.Dtos
{
    public class ImportQuestionsRequestDto
    {
        public string RawText { get; set; } = string.Empty;
        public Guid GroupId { get; set; }
    }
}
