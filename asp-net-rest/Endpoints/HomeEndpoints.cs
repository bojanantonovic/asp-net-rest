using AspNetRest.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AspNetRest.Endpoints;

public static class HomeEndpoints
{
    private const string ApiName = "ASP.NET Core Minimal API – FluentValidation & Dependency Injection";

    public static IEndpointRouteBuilder MapHomeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", GetOverview)
            .WithName(nameof(GetOverview))
            .WithSummary("Übersicht über die verfügbaren Endpunkte")
            .WithTags("Start");

        return endpoints;
    }

    private static Ok<ApiOverview> GetOverview(IWebHostEnvironment environment) =>
        TypedResults.Ok(new ApiOverview(ApiName, environment.EnvironmentName, BuildEndpointList()));

    private static IReadOnlyList<string> BuildEndpointList() =>
    [
        "GET /api/books",
        "GET /api/books/1",
        "POST /api/books",
        "GET /api/diagnostics/lifetimes",
        "GET /api/diagnostics/audit",
        "GET /api/diagnostics/boom",
        "GET /openapi/v1.json"
    ];
}
