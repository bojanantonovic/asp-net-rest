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
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddApplicationServices()
        {
            builder.Services
                .AddApplicationCoreServices(builder.Configuration)
                .AddWebInfrastructure();

            return builder;
        }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplicationCoreServices(IConfiguration configuration) =>
            services
                .AddConfiguredOptions(configuration)
                .AddPersistence()
                .AddDomainServices()
                .AddSortingStrategies()
                .AddLifetimeProbes()
                .AddFluentValidation();

        private IServiceCollection AddConfiguredOptions(IConfiguration configuration)
        {
            services.AddOptions<BookLibraryOptions>()
                .Bind(configuration.GetSection(BookLibraryOptions.SectionName))
                .ValidateOnStart();

            return services
                .AddSingleton<IValidateOptions<BookLibraryOptions>, FluentValidationOptions<BookLibraryOptions>>();
        }

        private IServiceCollection AddPersistence() =>
            services
                .AddSingleton<InMemoryBookRepository>()
                .AddSingleton<IBookRepository>(CreateLoggingRepository);

        private IServiceCollection AddDomainServices() =>
            services
                .AddSingleton<IClock, SystemClock>()
                .AddScoped<IAuditTrail, AuditTrail>()
                .AddScoped<IBookService, BookService>()
                .AddScoped<LifetimeInspector>()
                .AddTransient<IBookMapper, BookMapper>()
                .AddTransient<BookSorterSelector>()
                .AddHostedService<StartupWarmupService>();

        private IServiceCollection AddSortingStrategies() =>
            services
                .AddKeyedSingleton<IBookSorter, SortBooksById>(SortKeys.ById)
                .AddKeyedSingleton<IBookSorter, SortBooksByTitle>(SortKeys.ByTitle)
                .AddKeyedSingleton<IBookSorter, SortBooksByYear>(SortKeys.ByYear);

        private IServiceCollection AddLifetimeProbes() =>
            services
                .AddSingleton<ISingletonProbe, SingletonProbe>()
                .AddScoped<IScopedProbe, ScopedProbe>()
                .AddTransient<ITransientProbe, TransientProbe>();

        private IServiceCollection AddFluentValidation()
        {
            ValidatorOptions.Global.PropertyNameResolver = ResolveCamelCasePropertyName;

            return services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>(ServiceLifetime.Scoped);
        }

        private IServiceCollection AddWebInfrastructure() =>
            services
                .AddProblemDetails()
                .AddExceptionHandler<GlobalExceptionHandler>()
                .AddOpenApi();
    }

    private static IBookRepository CreateLoggingRepository(IServiceProvider services) =>
        new LoggingBookRepository(
            services.GetRequiredService<InMemoryBookRepository>(),
            services.GetRequiredService<ILogger<LoggingBookRepository>>());

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
}
