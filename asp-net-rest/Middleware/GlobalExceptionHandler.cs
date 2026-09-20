using Microsoft.AspNetCore.Diagnostics;

namespace AspNetRest.Middleware;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string LogMessage = "Unhandled exception during {Method} {Path}";
    private const string ProblemTitle = "Unexpected error";
    private const string ProblemDetail = "The request could not be processed.";

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
