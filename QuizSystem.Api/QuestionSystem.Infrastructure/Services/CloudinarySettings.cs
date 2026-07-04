namespace QuizSystem.Api.QuestionSystem.Infrastructure.Services;

public sealed class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Folder prefix inside the Cloudinary account, keeps quiz images
    /// separated from anything else stored under the same account.
    /// </summary>
    public string UploadFolder { get; set; } = "quiz-system";
}
