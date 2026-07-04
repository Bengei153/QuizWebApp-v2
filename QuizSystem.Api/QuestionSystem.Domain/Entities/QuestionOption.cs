using System;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Domain.Entities;

public class QuestionOption : BaseEntity
{
    public string Text { get; private set; } = null!;
    public bool isCorrect { get; set; }

    /// <summary>
    /// Cloud-hosted image for this option (e.g. Cloudinary secure URL). Optional —
    /// options can have text, an image, or both.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Provider-specific identifier (e.g. Cloudinary public_id) needed to delete
    /// or transform the image later.
    /// </summary>
    public string? ImagePublicId { get; private set; }

    public int? ImageWidth { get; private set; }
    public int? ImageHeight { get; private set; }

    private QuestionOption() { }

    public QuestionOption(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Attach or replace this option's image. Aspect-ratio consistency across
    /// sibling options is enforced by the owning Question, not here, since this
    /// entity has no visibility into its siblings.
    /// </summary>
    public void SetImage(string url, string publicId, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL is required");
        if (width <= 0 || height <= 0)
            throw new DomainException("Image dimensions must be positive");

        ImageUrl = url;
        ImagePublicId = publicId;
        ImageWidth = width;
        ImageHeight = height;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearImage()
    {
        ImageUrl = null;
        ImagePublicId = null;
        ImageWidth = null;
        ImageHeight = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aspect ratio (width / height) of this option's image, or null if it has none.
    /// </summary>
    public double? ImageAspectRatio =>
        ImageWidth is > 0 && ImageHeight is > 0
            ? (double)ImageWidth.Value / ImageHeight.Value
            : null;

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
