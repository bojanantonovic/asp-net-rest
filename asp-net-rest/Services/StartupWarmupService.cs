using AspNetRest.Contracts;

namespace AspNetRest.Services;

public sealed class StartupWarmupService(
    IServiceScopeFactory scopeFactory,
    ILogger<StartupWarmupService> logger) : IHostedService
{
    private const string StartedMessage = "Bestand beim Start: {Count} Bücher";
    private const string StoppedMessage = "Anwendung wird beendet – der In-Memory-Bestand geht verloren.";
    private const int FirstPage = 1;
    private const int ProbePageSize = 1;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var books = scope.ServiceProvider.GetRequiredService<IBookService>();
        var page = books.GetPage(new BookPageQuery(FirstPage, ProbePageSize, SortKeys.ById));

        logger.LogInformation(StartedMessage, page.TotalCount);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(StoppedMessage);

        return Task.CompletedTask;
    }
}
