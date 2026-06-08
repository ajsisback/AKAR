using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Akar.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // Known error codes that should map to 404 Not Found
    private static readonly HashSet<string> NotFoundCodes = new(StringComparer.Ordinal)
    {
        "OWNER_NOT_FOUND",
        "PROJECT_NOT_FOUND",
        "FOLDER_NOT_FOUND",
        "FILE_NOT_FOUND",
        "CONTRACT_NOT_FOUND",
        "FOLLOWER_NOT_FOUND",
        "UPLOAD_LINK_NOT_FOUND"
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation exception: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, "VALIDATION_ERROR", ex.Errors.Select(e => e.ErrorMessage).ToArray());
        }
        catch (InvalidOperationException ex) when (!string.IsNullOrEmpty(ex.Message) && ex.Message == ex.Message.ToUpperInvariant().Replace(' ', '_'))
        {
            // Business logic exceptions using error-code-style messages (e.g., "PROJECT_NOT_FOUND")
            // are mapped to safe HTTP responses instead of 500.
            var statusCode = NotFoundCodes.Contains(ex.Message) ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest;
            _logger.LogWarning("Business exception: {ErrorCode}", ex.Message);
            await WriteErrorResponse(context, statusCode, ex.Message, [ex.Message]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "INTERNAL_ERROR", ["An unexpected error occurred"]);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string errorCode, string[] errors)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = errorCode,
            Detail = string.Join("; ", errors),
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}

