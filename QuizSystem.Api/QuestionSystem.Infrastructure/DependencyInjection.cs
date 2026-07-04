using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.AI;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Storage;
using QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;
using QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;
using QuizSystem.Api.QuestionSystem.Infrastructure.Services;
using System;

namespace QuizSystem.Api.QuestionSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));


        services.AddScoped<IQuestionGroupRepository, QuestionGroupRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IAnswerRepository, AnswerRepository>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<IOptionRepository, OptionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Image storage (Cloudinary). Real credentials must come from
        // user-secrets locally or environment variables in deployment —
        // never commit them to appsettings.json.
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));
        services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();

        //AIII!!!!!
        services.Configure<AnthropicSettings>(configuration.GetSection(AnthropicSettings.SectionName));
        services.AddHttpClient<IQuizGenerationService, AnthropicQuizGenerationService>();

        return services;
    }
}
