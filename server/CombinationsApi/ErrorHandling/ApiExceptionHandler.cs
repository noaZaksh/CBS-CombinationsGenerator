using CombinationsCore.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CombinationsApi.ErrorHandling;

/// <summary>
/// Turns exceptions into clear HTTP answers in one place,
/// so the controller does not need try/catch in every action.
/// </summary>
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            SessionNotFoundException => (StatusCodes.Status404NotFound, "Session not found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid input"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error"),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unexpected error");
        }

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message,
        }, cancellationToken);

        return true;
    }
}
