using AspNetRest.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetRest.Tests.Fixtures;

public static class TestHost
{
    public static ServiceProvider CreateServiceProvider() =>
        new ServiceCollection()
            .AddLogging()
            .AddApplicationCoreServices(new ConfigurationBuilder().Build())
            .BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
}
