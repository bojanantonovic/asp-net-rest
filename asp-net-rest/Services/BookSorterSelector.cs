using Microsoft.Extensions.DependencyInjection;

namespace AspNetRest.Services;

public sealed class BookSorterSelector(IServiceProvider services)
{
    public IBookSorter Select(string? sortKey) =>
        services.GetRequiredKeyedService<IBookSorter>(SortKeys.Normalize(sortKey));
}
