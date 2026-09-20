using AspNetRest.Domain;
using AspNetRest.Services;
using AspNetRest.Tests.Fixtures;

namespace AspNetRest.Tests.Services;

public sealed class BookSorterTests
{
    [Fact]
    public void WhenSortingById_ThenBooksAreInInsertionOrder()
    {
        var sorter = new SortBooksById();

        var sorted = sorter.Sort(BookFixtures.UnsortedBooks());

        sorted.Select(GetId).Should().Equal(1, 2, 3);
    }

    [Fact]
    public void WhenSortingByTitle_ThenBooksAreInAlphabeticalOrder()
    {
        var sorter = new SortBooksByTitle();

        var sorted = sorter.Sort(BookFixtures.UnsortedBooks());

        sorted.Select(GetTitle).Should().Equal("Der Steppenwolf", "Die Physiker", "Homo Faber");
    }

    [Fact]
    public void WhenSortingByYear_ThenOldestBookComesFirst()
    {
        var sorter = new SortBooksByYear();

        var sorted = sorter.Sort(BookFixtures.UnsortedBooks());

        sorted.Select(GetYear).Should().Equal(1927, 1957, 1962);
    }

    [Fact]
    public void WhenSorting_ThenTheInputStaysUntouched()
    {
        var books = BookFixtures.UnsortedBooks();

        new SortBooksByTitle().Sort(books);

        books.Select(GetId).Should().Equal(3, 1, 2);
    }

    private static int GetId(Book book) => book.Id;

    private static string GetTitle(Book book) => book.Title;

    private static int GetYear(Book book) => book.Year;
}
