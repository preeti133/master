using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RallyAPI.SharedKernel.Results;
using System.Text.Json;

namespace RallyAPI.SharedKernel.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches unhandled exceptions and returns a consistent <see cref="ApiErrorResponse"/> JSON body.
/// Never leaks stack traces or internal exception details to clients.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            traceId,
            context.Request.Path,
            context.Request.Method);

        var (statusCode, errorCode, message) = exception switch
        {
            ArgumentNullException  => (StatusCodes.Status400BadRequest,  "BadRequest",   "Invalid request parameters."),
            ArgumentException      => (StatusCodes.Status400BadRequest,  "BadRequest",   "Invalid request."),
            UnauthorizedAccessException
                                   => (StatusCodes.Status401Unauthorized,"Unauthorized", "Unauthorized."),
            KeyNotFoundException   => (StatusCodes.Status404NotFound,    "NotFound",     "Resource not found."),
            OperationCanceledException
                                   => (499,                              "Cancelled",    "Request cancelled."),
            _                      => (StatusCodes.Status500InternalServerError, "InternalError", "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        // TraceId in the response so clients can report it without needing stack traces
        var response = new ApiErrorResponse(errorCode, message,
            new[] { new FieldError("traceId", traceId) });

        await context.Response.WriteAsJsonAsync(response, JsonOptions);
    }
}
