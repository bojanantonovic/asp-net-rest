using AspNetRest.Contracts;

namespace AspNetRest.Services.Diagnostics;

public sealed class LifetimeInspector(IServiceProvider scopeServices, IServiceScopeFactory scopeFactory)
{
    private const string RequestScopeName = "Request-Scope (von ASP.NET pro Request geöffnet)";
    private const string ChildScopeName = "Eigener Scope (mit IServiceScopeFactory erzeugt)";

    private const string SingletonConclusion =
        "Singleton: in beiden Scopes dieselbe Id – eine Instanz für die gesamte Laufzeit.";

    private const string ScopedConclusion =
        "Scoped: innerhalb eines Scopes dieselbe Id, im zweiten Scope eine andere.";

    private const string TransientConclusion =
        "Transient: schon zwei Auflösungen im selben Scope liefern verschiedene Ids.";

    private const string DisposeConclusion =
        "Beim Verlassen des Scopes (using) gibt der Container alle IDisposable-Services dieses Scopes frei.";

    public LifetimeReport Inspect()
    {
        var requestScope = TakeSnapshot(scopeServices, RequestScopeName);

        using var childScope = scopeFactory.CreateScope();
        var child = TakeSnapshot(childScope.ServiceProvider, ChildScopeName);

        return new LifetimeReport(requestScope, child, BuildConclusions());
    }

    private static ScopeSnapshot TakeSnapshot(IServiceProvider services, string scopeName) =>
        new(
            scopeName,
            services.GetRequiredService<ISingletonProbe>().InstanceId,
            services.GetRequiredService<IScopedProbe>().InstanceId,
            services.GetRequiredService<ITransientProbe>().InstanceId,
            services.GetRequiredService<ITransientProbe>().InstanceId);

    private static IReadOnlyList<string> BuildConclusions() =>
        [SingletonConclusion, ScopedConclusion, TransientConclusion, DisposeConclusion];
}
