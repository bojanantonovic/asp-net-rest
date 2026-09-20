using System.Linq.Expressions;
using System.Reflection;
using AspNetRest.Middleware;
using AspNetRest.Options;
using AspNetRest.Persistence;
using AspNetRest.Services;
using AspNetRest.Services.Diagnostics;
using AspNetRest.Validation;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AspNetRest.Composition;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddApplicationCoreServices(builder.Configuration)
            .AddWebInfrastructure();

        return builder;
    }

    public static IServiceCollection AddApplicationCoreServices(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddConfiguredOptions(configuration)
            .AddPersistence()
            .AddDomainServices()
            .AddSortingStrategies()
            .AddLifetimeProbes()
            .AddFluentValidation();

    private static IServiceCollection AddConfiguredOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<BookLibraryOptions>()
            .Bind(configuration.GetSection(BookLibraryOptions.SectionName))
            .ValidateOnStart();

        return services
            .AddSingleton<IValidateOptions<BookLibraryOptions>, FluentValidationOptions<BookLibraryOptions>>();
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services) =>
        services
            .AddSingleton<InMemoryBookRepository>()
            .AddSingleton<IBookRepository>(CreateLoggingRepository);

    private static IBookRepository CreateLoggingRepository(IServiceProvider services) =>
        new LoggingBookRepository(
            services.GetRequiredService<InMemoryBookRepository>(),
            services.GetRequiredService<ILogger<LoggingBookRepository>>());

    private static IServiceCollection AddDomainServices(this IServiceCollection services) =>
        services
            .AddSingleton<IClock, SystemClock>()
            .AddScoped<IAuditTrail, AuditTrail>()
            .AddScoped<IBookService, BookService>()
            .AddScoped<LifetimeInspector>()
            .AddTransient<IBookMapper, BookMapper>()
            .AddTransient<BookSorterSelector>()
            .AddHostedService<StartupWarmupService>();

    private static IServiceCollection AddSortingStrategies(this IServiceCollection services) =>
        services
            .AddKeyedSingleton<IBookSorter, SortBooksById>(SortKeys.ById)
            .AddKeyedSingleton<IBookSorter, SortBooksByTitle>(SortKeys.ByTitle)
            .AddKeyedSingleton<IBookSorter, SortBooksByYear>(SortKeys.ByYear);

    private static IServiceCollection AddLifetimeProbes(this IServiceCollection services) =>
        services
            .AddSingleton<ISingletonProbe, SingletonProbe>()
            .AddScoped<IScopedProbe, ScopedProbe>()
            .AddTransient<ITransientProbe, TransientProbe>();

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        ValidatorOptions.Global.PropertyNameResolver = ResolveCamelCasePropertyName;

        return services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>(ServiceLifetime.Scoped);
    }

    private static string ResolveCamelCasePropertyName(
        Type? type,
        MemberInfo? member,
        LambdaExpression? expression)
    {
        var name = member?.Name;

        return string.IsNullOrEmpty(name)
            ? string.Empty
            : char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static IServiceCollection AddWebInfrastructure(this IServiceCollection services) =>
        services
            .AddProblemDetails()
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddOpenApi();
}
