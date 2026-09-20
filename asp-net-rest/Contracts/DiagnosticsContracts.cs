namespace AspNetRest.Contracts;

public sealed record ScopeSnapshot(
    string ScopeName,
    Guid Singleton,
    Guid Scoped,
    Guid TransientFirst,
    Guid TransientSecond);

public sealed record LifetimeReport(
    ScopeSnapshot RequestScope,
    ScopeSnapshot ChildScope,
    IReadOnlyList<string> Conclusions);

public sealed record ApiOverview(string Name, string Environment, IReadOnlyList<string> Endpoints);
