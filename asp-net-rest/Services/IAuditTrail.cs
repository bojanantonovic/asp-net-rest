namespace AspNetRest.Services;

public sealed record AuditEntry(DateTimeOffset Timestamp, string Step);

public interface IAuditTrail
{
    IReadOnlyList<AuditEntry> Entries { get; }

    IAuditTrail Record(string step);
}

public sealed class AuditTrail(IClock clock) : IAuditTrail
{
    private readonly List<AuditEntry> _entries = [];

    public IReadOnlyList<AuditEntry> Entries => _entries;

    public IAuditTrail Record(string step)
    {
        _entries.Add(new AuditEntry(clock.UtcNow, step));

        return this;
    }
}
