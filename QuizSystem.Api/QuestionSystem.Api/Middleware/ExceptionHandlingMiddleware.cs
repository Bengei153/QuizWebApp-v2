using System.Net;
using System.Text.Json;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Api.Middleware;

/// <summary>
/// Central exception handler. Maps known exception types to the correct HTTP
/// status code instead of returning 500 for everything, and avoids leaking
/// internal exception details (stack traces, DB error text, etc.) outside
/// Development.
///
/// NOTE: Several handlers currently throw InvalidOperationException for both
/// "resource not found" AND "user not authenticated" cases. Longer term this
/// should be split into dedicated NotFoundException / UnauthenticatedException
/// types; for now InvalidOperationException maps to 404 since that's the more
/// common case in this codebase.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, title) = MapException(ex);

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled exception");
            else
                _logger.LogWarning(ex, "Handled exception: {Title}", title);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            // Only surface the raw exception message for expected/handled
            // exceptions, or when running in Development. Unexpected 500s
            // in other environments get a generic message so we don't leak
            // internals (DB connection errors, stack details, etc.).
            var detail = statusCode != HttpStatusCode.InternalServerError || _environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred. Please try again later.";

            var response = new
            {
                title,
                status = (int)statusCode,
                detail
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

    private static (HttpStatusCode StatusCode, string Title) MapException(Exception ex) => ex switch
    {
        ForbiddenAccessException => (HttpStatusCode.Forbidden, "Forbidden"),
        UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
        KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
        FluentValidation.ValidationException => (HttpStatusCode.BadRequest, "Validation Failed"),
        DomainException => (HttpStatusCode.BadRequest, "Bad Request"),
        InvalidOperationException => (HttpStatusCode.NotFound, "Not Found"),
        ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
        _ => (HttpStatusCode.InternalServerError, "An error occurred")
    };
}
