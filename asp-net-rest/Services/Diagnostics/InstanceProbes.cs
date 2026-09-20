namespace AspNetRest.Services.Diagnostics;

public interface IInstanceProbe
{
    Guid InstanceId { get; }
}

public interface ISingletonProbe : IInstanceProbe;

public interface IScopedProbe : IInstanceProbe;

public interface ITransientProbe : IInstanceProbe;

public abstract class InstanceProbe : IInstanceProbe
{
    public Guid InstanceId { get; } = Guid.NewGuid();
}

public sealed class SingletonProbe : InstanceProbe, ISingletonProbe;

public sealed class ScopedProbe : InstanceProbe, IScopedProbe;

public sealed class TransientProbe : InstanceProbe, ITransientProbe;
