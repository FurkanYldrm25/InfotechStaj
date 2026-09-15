// Staj.API/Middleware/ExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Staj.API.Middleware;

// Global exception yakalama middleware
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Doğrulama hatası oluştu.");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                success = false,
                message = "Doğrulama hatası.",
                errors = vex.Errors.Select(e => e.ErrorMessage)
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen hata oluştu.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                success = false,
                message = "Sunucu hatası oluştu.",
                errors = new[] { ex.Message }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}