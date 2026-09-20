using AspNetRest.Domain;
using AspNetRest.Options;
using Microsoft.Extensions.Options;

namespace AspNetRest.Tests.Fixtures;

public static class BookFixtures
{
    public const int SeededBookCount = 3;

    public const int FirstBookId = 1;

    public const int UnknownBookId = 999;

    public const string SeededTitle = "Der Steppenwolf";
    public const string SeededAuthor = "Hermann Hesse";
    public const int SeededYear = 1927;

    public const string NewTitle = "Der Richter und sein Henker";

    public const string NewAuthor = "Friedrich Dürrenmatt";
    public const int NewYear = 1950;

    public const string OtherTitle = "Der Verdacht";

    public const int OtherYear = 1951;

    public static IOptions<BookLibraryOptions> Limits(BookLibraryOptions? limits = null) =>
        Microsoft.Extensions.Options.Options.Create(limits ?? new BookLibraryOptions());

    public static IReadOnlyList<Book> UnsortedBooks() =>
    [
        new(3, "Homo Faber", "Max Frisch", 1957),
        new(1, SeededTitle, SeededAuthor, SeededYear),
        new(2, "Die Physiker", NewAuthor, 1962)
    ];
}
