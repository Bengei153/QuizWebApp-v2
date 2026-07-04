using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Storage;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using ImageUploadResult = QuizSystem.Api.QuestionSystem.Application.Abstractions.Storage.ImageUploadResult;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Services;

public sealed class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly string _uploadFolder;

    public CloudinaryImageStorageService(IOptions<CloudinarySettings> settings)
    {
        var config = settings.Value;

        if (string.IsNullOrWhiteSpace(config.CloudName) ||
            string.IsNullOrWhiteSpace(config.ApiKey) ||
            string.IsNullOrWhiteSpace(config.ApiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary is not configured. Set Cloudinary:CloudName, Cloudinary:ApiKey and " +
                "Cloudinary:ApiSecret (via user-secrets or environment variables).");
        }

        _cloudinary = new Cloudinary(new Account(config.CloudName, config.ApiKey, config.ApiSecret))
        {
            Api = { Secure = true }
        };
        _uploadFolder = config.UploadFolder;
    }

    public async Task<ImageUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, content),
            Folder = _uploadFolder,
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error is not null)
            throw new DomainException($"Image upload failed: {result.Error.Message}");

        return new ImageUploadResult(
            result.SecureUrl.ToString(),
            result.PublicId,
            result.Width,
            result.Height);
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var deleteParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deleteParams);
    }
}
