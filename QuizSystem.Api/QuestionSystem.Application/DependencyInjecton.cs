using System;
using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Features.Answers;
using QuizSystem.Api.QuestionSystem.Application.Features.Admin;
using QuizSystem.Api.QuestionSystem.Application.Features.Folders;
using QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.CreateQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.UpdateQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Quiz;
using QuizSystem.Api.QuestionSystem.Application.Security;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestion;

namespace QuizSystem.Api.QuestionSystem.Application;

/// <summary>
/// Dependency injection configuration for the Application layer.
/// 
/// This registers:
/// - Handler services (business logic)
/// - Command services
/// - Security services (OwnershipGuard)
/// - Authorization helpers
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Register all handlers and security services.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetFolderHandler>();
        services.AddScoped<GetAllFolderHandler>();
        services.AddScoped<CreateFolderHandler>();
        services.AddScoped<CreateQuestionGroupHandler>();
        services.AddScoped<AddOptionHandler>();
        services.AddScoped<GetQuestionHandler>();
        services.AddScoped<Features.Questions.CreateQuestion.CreateQuestionHandler>();
        services.AddScoped<Features.Questions.UpdateQuestion.UpdateQuestionHandler>();
        services.AddScoped<Features.Questions.DeleteQuestion.DeleteQuestionHandler>();
        services.AddScoped<Features.Questions.Options.RemoveOptionHandler>();
        services.AddScoped<Features.Questions.Image.UploadQuestionImageHandler>();
        services.AddScoped<Features.Questions.Image.DeleteQuestionImageHandler>();
        services.AddScoped<Features.Questions.Options.UploadOptionImageHandler>();
        services.AddScoped<Features.Questions.Options.DeleteOptionImageHandler>();
        services.AddScoped<GetAllQuestionsHandler>();
        services.AddScoped<GetQuestionGroupHandler>();
        services.AddScoped<GetMyQuestionGroupsHandler>();
        services.AddScoped<UpdateQuestionGroupHandler>();
        services.AddScoped<SubmitAnswerHandler>();
        services.AddScoped<DeleteQuestionGroupHandler>();

        services.AddMediatR(typeof(GetAttemptDetailsHandler).Assembly);
        services.AddMediatR(typeof(GetAttemptDetailsHandler).Assembly);
        services.AddMediatR(typeof(SubmitAnswerHandler).Assembly);
        services.AddMediatR(typeof(SaveAnswerHandler).Assembly);

        // NEW: Enhanced handlers with authorization
        services.AddScoped<UpdateFolderHandlerWithAuth>();
        services.AddScoped<DeleteFolderHandlerWithAuth>();

        // Security & Authorization
        services.AddScoped<OwnershipGuard>();

        //Antropic import questions
        services.AddScoped<Features.Questions.Import.ImportQuestionsHandler>();


        return services;
    }

    /// <summary>
    /// Register all command services.
    /// </summary>
    public static IServiceCollection AddApplicationCommand(this IServiceCollection services)
    {
        services.AddScoped<GetQuestionQuery>();

        return services;
    }
}


