using AspNetRest.Services;

namespace AspNetRest.Middleware;

public sealed class AuditTrailMiddleware(RequestDelegate next, ILogger<AuditTrailMiddleware> logger)
{
    private const string StartStep = "Request started: {0} {1}";
    private const string AuditMessage = "{Method} {Path} -> {StatusCode}; steps: {Steps}";
    private const string StepSeparator = " | ";

    public Task InvokeAsync(HttpContext context, IAuditTrail auditTrail)
    {
        auditTrail.Record(string.Format(StartStep, context.Request.Method, context.Request.Path));
        context.Response.OnCompleted(WriteAuditLog, new AuditLogState(logger, context, auditTrail));

        return next(context);
    }

    private static Task WriteAuditLog(object state)
    {
        var (logger, context, auditTrail) = (AuditLogState)state;

        logger.LogInformation(
            AuditMessage,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            string.Join(StepSeparator, auditTrail.Entries.Select(GetStep)));

        return Task.CompletedTask;
    }

    private static string GetStep(AuditEntry entry) => entry.Step;

    private sealed record AuditLogState(ILogger Logger, HttpContext Context, IAuditTrail AuditTrail);
}
