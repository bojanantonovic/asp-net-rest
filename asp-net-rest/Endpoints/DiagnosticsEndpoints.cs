using AspNetRest.Contracts;
using AspNetRest.Services;
using AspNetRest.Services.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AspNetRest.Endpoints;

public static class DiagnosticsEndpoints
{
    public static IEndpointRouteBuilder MapDiagnosticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var diagnostics = endpoints.MapGroup("/api/diagnostics").WithTags("Diagnostics");

        diagnostics.MapGet("/lifetimes", GetLifetimes)
            .WithName(nameof(GetLifetimes))
            .WithSummary("Compare the three lifetimes across two scopes");

        diagnostics.MapGet("/audit", GetAuditTrail)
            .WithName(nameof(GetAuditTrail))
            .WithSummary("Read the audit trail of the current request");

        diagnostics.MapGet("/boom", Boom)
            .WithName(nameof(Boom))
            .WithSummary("Throw an unhandled exception (results in 500)");

        return endpoints;
    }

    private static Ok<LifetimeReport> GetLifetimes(LifetimeInspector inspector) =>
        TypedResults.Ok(inspector.Inspect());

    private static Ok<IReadOnlyList<AuditEntry>> GetAuditTrail(IAuditTrail auditTrail) =>
        TypedResults.Ok(auditTrail.Entries);

    private static IResult Boom() =>
        throw new InvalidOperationException("Thrown on purpose to demonstrate the error handling.");
}
