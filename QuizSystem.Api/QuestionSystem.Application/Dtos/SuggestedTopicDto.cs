namespace QuizSystem.Api.QuestionSystem.Application.Dtos
{
    public class SuggestedTopicDto
    {
        public Guid Id { get; set; }
        public string Tag { get; set; } = "WEAK POINT";
        public string Title { get; set; } = "";
        public string Desc { get; set; } = "";
        public Guid GroupId { get; set; }
    }
}
