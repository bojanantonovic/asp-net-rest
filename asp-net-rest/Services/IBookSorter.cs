using AspNetRest.Domain;

namespace AspNetRest.Services;

public static class SortKeys
{
    public const string ById = "id";

    public const string ByTitle = "title";

    public const string ByYear = "year";

    public static readonly IReadOnlyList<string> All = [ById, ByTitle, ByYear];

    public static bool IsKnown(string? key) =>
        key is not null && All.Contains(key, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string? key) =>
        All.FirstOrDefault(known => string.Equals(known, key, StringComparison.OrdinalIgnoreCase)) ?? ById;
}

public interface IBookSorter
{
    IReadOnlyList<Book> Sort(IEnumerable<Book> books);
}

public sealed class SortBooksById : IBookSorter
{
    public IReadOnlyList<Book> Sort(IEnumerable<Book> books) => books.OrderBy(GetId).ToList();

    private static int GetId(Book book) => book.Id;
}

public sealed class SortBooksByTitle : IBookSorter
{
    public IReadOnlyList<Book> Sort(IEnumerable<Book> books) =>
        books.OrderBy(GetTitle, StringComparer.CurrentCultureIgnoreCase).ToList();

    private static string GetTitle(Book book) => book.Title;
}

public sealed class SortBooksByYear : IBookSorter
{
    public IReadOnlyList<Book> Sort(IEnumerable<Book> books) =>
        books.OrderBy(GetYear).ThenBy(GetTitle, StringComparer.CurrentCultureIgnoreCase).ToList();

    private static int GetYear(Book book) => book.Year;

    private static string GetTitle(Book book) => book.Title;
}
