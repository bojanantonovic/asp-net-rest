using AspNetRest.Contracts;
using AspNetRest.Services;
using AspNetRest.Services.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AspNetRest.Endpoints;

public static class DiagnosticsEndpoints
{
    public static IEndpointRouteBuilder MapDiagnosticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var diagnostics = endpoints.MapGroup("/api/diagnostics").WithTags("Diagnose");

        diagnostics.MapGet("/lifetimes", GetLifetimes)
            .WithName(nameof(GetLifetimes))
            .WithSummary("Die drei Lebensdauern in zwei Scopes vergleichen");

        diagnostics.MapGet("/audit", GetAuditTrail)
            .WithName(nameof(GetAuditTrail))
            .WithSummary("Das Protokoll des aktuellen Requests lesen");

        diagnostics.MapGet("/boom", Boom)
            .WithName(nameof(Boom))
            .WithSummary("Eine unbehandelte Ausnahme auslösen (ergibt 500)");

        return endpoints;
    }

    private static Ok<LifetimeReport> GetLifetimes(LifetimeInspector inspector) =>
        TypedResults.Ok(inspector.Inspect());

    private static Ok<IReadOnlyList<AuditEntry>> GetAuditTrail(IAuditTrail auditTrail) =>
        TypedResults.Ok(auditTrail.Entries);

    private static IResult Boom() =>
        throw new InvalidOperationException("Absichtlich ausgelöst, um die Fehlerbehandlung zu zeigen.");
}
