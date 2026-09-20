using Microsoft.AspNetCore.Diagnostics;

namespace AspNetRest.Middleware;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string LogMessage = "Unbehandelte Ausnahme bei {Method} {Path}";
    private const string ProblemTitle = "Unerwarteter Fehler";
    private const string ProblemDetail = "Die Anfrage konnte nicht verarbeitet werden.";

    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, LogMessage, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Title = ProblemTitle,
                Detail = ProblemDetail,
                Status = StatusCodes.Status500InternalServerError
            }
        });
    }
}
