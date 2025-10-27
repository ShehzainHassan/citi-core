using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using citi_core.Models;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex, _env);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
    {
        if (context.Response.HasStarted)
            return;

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            System.ComponentModel.DataAnnotations.ValidationException =>
                (StatusCodes.Status400BadRequest, "Validation failed"),

            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized"),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found"),

            _ =>
                (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        response.StatusCode = statusCode;

        var error = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Details = env.IsDevelopment() ? exception.ToString() : null
        };

        var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(json);
    }
}
