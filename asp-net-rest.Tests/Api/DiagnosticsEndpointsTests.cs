using System.Net;
using System.Net.Http.Json;
using AspNetRest.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AspNetRest.Tests.Api;

public sealed class DiagnosticsEndpointsTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public DiagnosticsEndpointsTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task WhenAskingTwice_ThenOnlyTheSingletonSurvivesBothRequests()
    {
        var firstReport = await _client.GetFromJsonAsync<LifetimeReport>("/api/diagnostics/lifetimes");
        var secondReport = await _client.GetFromJsonAsync<LifetimeReport>("/api/diagnostics/lifetimes");

        firstReport.Should().NotBeNull();
        secondReport.Should().NotBeNull();

        secondReport.RequestScope.Singleton.Should().Be(firstReport.RequestScope.Singleton);
        secondReport.RequestScope.Scoped.Should().NotBe(firstReport.RequestScope.Scoped);
    }

    [Fact]
    public async Task WhenAskingOnce_ThenTheChildScopeBehavesAsDocumented()
    {
        var report = await _client.GetFromJsonAsync<LifetimeReport>("/api/diagnostics/lifetimes");

        report.Should().NotBeNull();
        report.ChildScope.Singleton.Should().Be(report.RequestScope.Singleton);
        report.ChildScope.Scoped.Should().NotBe(report.RequestScope.Scoped);
        report.ChildScope.TransientSecond.Should().NotBe(report.ChildScope.TransientFirst);
    }

    [Fact]
    public async Task WhenReadingTheAuditTrail_ThenTheMiddlewareEntryIsAlreadyThere()
    {
        var entries = await _client.GetFromJsonAsync<IReadOnlyList<AuditEntryResponse>>("/api/diagnostics/audit");

        entries.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GivenAFailingEndpoint_WhenCallingIt_ThenProblemDetailsAreReturned()
    {
        var response = await _client.GetAsync("/api/diagnostics/boom");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private sealed record AuditEntryResponse(DateTimeOffset Timestamp, string Step);
}
