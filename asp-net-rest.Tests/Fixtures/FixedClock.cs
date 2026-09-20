using AspNetRest.Services;

namespace AspNetRest.Tests.Fixtures;

public sealed class FixedClock(DateTimeOffset now) : IClock
{
    public const int Year = 2025;

    public static FixedClock Default => new(new DateTimeOffset(Year, 7, 1, 12, 0, 0, TimeSpan.Zero));

    public DateTimeOffset UtcNow { get; } = now;
}
