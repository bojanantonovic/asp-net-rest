using AspNetRest.Contracts;

namespace AspNetRest.Services.Diagnostics;

public sealed class LifetimeInspector(IServiceProvider scopeServices, IServiceScopeFactory scopeFactory)
{
    private const string RequestScopeName = "Request scope (opened by ASP.NET for every request)";
    private const string ChildScopeName = "Own scope (created with IServiceScopeFactory)";

    private const string SingletonConclusion =
        "Singleton: the same id in both scopes - one instance for the whole application lifetime.";

    private const string ScopedConclusion =
        "Scoped: the same id within one scope, a different one in the second scope.";

    private const string TransientConclusion =
        "Transient: even two resolutions within the same scope return different ids.";

    private const string DisposeConclusion =
        "When the scope is disposed, the container releases every IDisposable service of that scope.";

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
