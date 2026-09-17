using System.Net;
using System.Text.Json;
using AgenticBoard.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AgenticBoard.Api.Middleware;

public class ExceptionHandlingMiddleware
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Validation Error",
                    Detail = validationEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["errors"] = validationEx.Errors }
                }
            ),
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Title = "Unauthorized",
                    Detail = unauthorizedEx.Message,
                    Instance = context.Request.Path
                }
            ),
            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Forbidden,
                    Title = "Forbidden",
                    Detail = forbiddenEx.Message,
                    Instance = context.Request.Path
                }
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Resource Not Found",
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path
                }
            ),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Conflict",
                    Detail = conflictEx.Message,
                    Instance = context.Request.Path
                }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path
                }
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred while processing request to {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Handled business exception ({StatusCode}): {Message} on path {Path}",
                (int)statusCode, exception.Message, context.Request.Path);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }
}
