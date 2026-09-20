using AspNetRest.Options;
using AspNetRest.Persistence;
using AspNetRest.Services;
using AspNetRest.Services.Diagnostics;
using AspNetRest.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AspNetRest.Tests.Services;

public sealed class ServiceLifetimeTests : IDisposable
{
    private readonly ServiceProvider _services = TestHost.CreateServiceProvider();

    [Fact]
    public void WhenResolvingASingletonInTwoScopes_ThenItIsTheSameInstance()
    {
        using var firstScope = _services.CreateScope();
        using var secondScope = _services.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<ISingletonProbe>();
        var second = secondScope.ServiceProvider.GetRequiredService<ISingletonProbe>();

        second.InstanceId.Should().Be(first.InstanceId);
    }

    [Fact]
    public void WhenResolvingAScopedServiceTwiceInOneScope_ThenItIsTheSameInstance()
    {
        using var scope = _services.CreateScope();

        var first = scope.ServiceProvider.GetRequiredService<IScopedProbe>();
        var second = scope.ServiceProvider.GetRequiredService<IScopedProbe>();

        second.InstanceId.Should().Be(first.InstanceId);
    }

    [Fact]
    public void WhenResolvingAScopedServiceInTwoScopes_ThenTheInstancesDiffer()
    {
        using var firstScope = _services.CreateScope();
        using var secondScope = _services.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<IScopedProbe>();
        var second = secondScope.ServiceProvider.GetRequiredService<IScopedProbe>();

        second.InstanceId.Should().NotBe(first.InstanceId);
    }

    [Fact]
    public void WhenResolvingATransientServiceTwice_ThenEveryResolutionIsNew()
    {
        using var scope = _services.CreateScope();

        var first = scope.ServiceProvider.GetRequiredService<ITransientProbe>();
        var second = scope.ServiceProvider.GetRequiredService<ITransientProbe>();

        second.InstanceId.Should().NotBe(first.InstanceId);
    }

    [Fact]
    public void WhenInspectingTheLifetimes_ThenTheReportMatchesTheRules()
    {
        using var scope = _services.CreateScope();
        var inspector = scope.ServiceProvider.GetRequiredService<LifetimeInspector>();

        var report = inspector.Inspect();

        report.ChildScope.Singleton.Should().Be(report.RequestScope.Singleton);
        report.ChildScope.Scoped.Should().NotBe(report.RequestScope.Scoped);
        report.RequestScope.TransientSecond.Should().NotBe(report.RequestScope.TransientFirst);
    }

    [Theory]
    [InlineData(SortKeys.ById, typeof(SortBooksById))]
    [InlineData(SortKeys.ByTitle, typeof(SortBooksByTitle))]
    [InlineData(SortKeys.ByYear, typeof(SortBooksByYear))]
    public void GivenASortKey_WhenResolvingTheKeyedService_ThenTheMatchingStrategyIsReturned(
        string sortKey,
        Type expectedType)
    {
        var sorter = _services.GetRequiredKeyedService<IBookSorter>(sortKey);

        sorter.Should().BeOfType(expectedType);
    }

    [Fact]
    public void WhenResolvingTheRepository_ThenTheLoggingDecoratorIsInFront()
    {
        var repository = _services.GetRequiredService<IBookRepository>();

        repository.Should().BeOfType<LoggingBookRepository>();
    }

    [Fact]
    public void WhenResolvingTheOptions_ThenTheConfiguredLimitsAreAvailable()
    {
        var limits = _services.GetRequiredService<IOptions<BookLibraryOptions>>().Value;

        limits.DefaultPageSize.Should().BePositive().And.BeLessThanOrEqualTo(limits.MaxPageSize);
        limits.MaxTitleLength.Should().BePositive();
    }

    [Fact]
    public void GivenTheHostedService_WhenResolvingIt_ThenItIsASingletonWithoutCaptiveDependencies()
    {
        var hostedServices = _services.GetServices<IHostedService>();

        hostedServices.OfType<StartupWarmupService>().Should().ContainSingle();
    }

    public void Dispose() => _services.Dispose();
}
