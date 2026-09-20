using AspNetRest.Contracts;

namespace AspNetRest.Services;

public sealed class StartupWarmupService(
    IServiceScopeFactory scopeFactory,
    ILogger<StartupWarmupService> logger) : IHostedService
{
    private const string StartedMessage = "Books in store at startup: {Count}";
    private const string StoppedMessage = "Application is shutting down - the in-memory store will be lost.";
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
