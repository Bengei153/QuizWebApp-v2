namespace QuizSystem.Api.QuestionSystem.Application.Abstractions.Storage;

/// <summary>
/// Abstraction over the image hosting provider. Implementations upload raw
/// bytes and return a public URL plus the dimensions of the stored image
/// (needed for the option aspect-ratio rule) and a provider-specific
/// identifier used to delete the asset later.
///
/// Swapping providers (Cloudinary -> S3 -> Azure Blob) only requires a new
/// implementation of this interface — nothing in the Application or Domain
/// layers needs to change.
/// </summary>
public interface IImageStorageService
{
    Task<ImageUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}

public sealed record ImageUploadResult(
    string Url,
    string PublicId,
    int Width,
    int Height);
