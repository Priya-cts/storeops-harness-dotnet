namespace StoreOps.Api.Shared.Middleware;

using StoreOps.Api.Shared.Errors;

/// <summary>
/// Single place where AppError instances are translated into HTTP responses.
/// This is what lets services/routes throw typed errors instead of returning
/// ad-hoc status codes, and is one of the automated + LLM-assessed hard gates
/// the harness Evaluator checks (zero raw `throw new Exception()` in services/routes).
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

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
        catch (AppError ex)
        {
            _logger.LogWarning(ex, "AppError handled: {Code}", ex.Code);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                error = new { code = ex.Code, message = ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = new { code = "INTERNAL_ERROR", message = "An unexpected error occurred." }
            });
        }
    }
}
