using System;
using System.Text.Json.Serialization;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Domain.Enums;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Domain.Entities;

/// <summary>
/// Question entity representing a quiz question with optional answers/options.
///
/// Security:
/// - CreatedByUserId: User who created this question
/// - IsDeleted + DeletedAt: Soft delete support
///
/// Media:
/// - A question always has Text, and may additionally have an image.
/// - Each option may have text, an image, or both. When multiple options on the
///   same question have images, those images must share the same aspect ratio
///   (enforced by <see cref="EnsureOptionImageAspectRatioIsConsistent"/>) so the
///   choices render uniformly in the UI.
/// </summary>
public class Question : BaseEntity
{
    private const double AspectRatioTolerance = 0.02;

    public QuestionText Text { get; private set; } = null!;
    public QuestionType Type { get; private set; }
    public Guid FolderId { get; private set; }

    public string? ImageUrl { get; private set; }
    public string? ImagePublicId { get; private set; }

    private readonly List<QuestionOption> _options = new();
    public IReadOnlyCollection<QuestionOption> Options => _options;

    /// <summary>
    /// User ID who created this question. Used for ownership authorization.
    /// </summary>
    public string CreatedByUserId { get; set; } = default!;

    private Question() { }

    public Question(QuestionText text, QuestionType type, Guid folderId)
    {
        Text = text;
        Type = type;
        FolderId = folderId;
        CreatedAt = DateTime.UtcNow;
    }

    public QuestionOption AddOption(string text, bool isCorrect = false)
    {
        if (Type == QuestionType.Text)
            throw new DomainException("Text questions cannot have options");

        var option = new QuestionOption(text) { isCorrect = isCorrect };
        _options.Add(option);
        return option;
    }

    /// <summary>
    /// Soft delete a single option belonging to this question.
    /// </summary>
    public void RemoveOption(Guid optionId)
    {
        var option = _options.FirstOrDefault(o => o.Id == optionId);
        if (option is null)
            throw new InvalidOperationException("Option not found on this question");

        option.SoftDelete();
    }

    /// <summary>
    /// Attach or replace the question's own image.
    /// </summary>
    public void SetImage(string url, string publicId)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL is required");

        ImageUrl = url;
        ImagePublicId = publicId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearImage()
    {
        ImageUrl = null;
        ImagePublicId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates that a candidate option image's aspect ratio matches every
    /// other option image already attached to this question. The first option
    /// image sets the standard; every subsequent one must match within a small
    /// tolerance (accounts for rounding during upload/processing).
    /// </summary>
    public void EnsureOptionImageAspectRatioIsConsistent(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new DomainException("Image dimensions must be positive");

        var candidateRatio = (double)width / height;

        var existingRatio = _options
            .Where(o => !o.IsDeleted)
            .Select(o => o.ImageAspectRatio)
            .FirstOrDefault(r => r.HasValue);

        if (existingRatio.HasValue &&
            Math.Abs(existingRatio.Value - candidateRatio) > AspectRatioTolerance)
        {
            throw new DomainException(
                $"This option's image aspect ratio ({width}x{height}) doesn't match the " +
                $"other options on this question. All option images must share the same aspect ratio.");
        }
    }

    /// <summary>
    /// Soft delete the question.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted question.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }

    /// <summary>
    /// Update question text with timestamp.
    /// </summary>
    public void UpdateText(QuestionText text)
    {
        Text = text;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ValidateAnswer(IReadOnlyCollection<AnswerValue> values)
    {
        if (Type == QuestionType.Text)
        {
            if (values.Count != 1 || values.Any(v => v.TextValue is null))
                throw new InvalidOperationException("Text question requires exactly one text answer.");
        }

        if (Type == QuestionType.SingleChoice)
        {
            if (values.Count != 1 || values.Any(v => v.OptionId is null))
                throw new InvalidOperationException("Single choice question requires exactly one option.");
        }

        if (Type == QuestionType.MultipleChoice)
        {
            if (!values.Any() || values.Any(v => v.OptionId is null))
                throw new InvalidOperationException("Multi choice question requires one or more options.");
        }
    }
}
