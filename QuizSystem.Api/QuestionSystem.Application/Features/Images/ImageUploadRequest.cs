using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Images;

/// <summary>
/// Transport-agnostic representation of an uploaded file, so Application-layer
/// handlers don't depend on ASP.NET Core's IFormFile.
/// </summary>
public sealed class ImageUploadRequest
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public required Stream Content { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long Length { get; init; }

    public void Validate()
    {
        if (Length <= 0)
            throw new DomainException("Uploaded file is empty");

        if (Length > MaxFileSizeBytes)
            throw new DomainException($"Image exceeds the {MaxFileSizeBytes / (1024 * 1024)}MB size limit");

        if (!AllowedContentTypes.Contains(ContentType))
            throw new DomainException(
                $"Unsupported image type '{ContentType}'. Allowed types: {string.Join(", ", AllowedContentTypes)}");
    }
}
