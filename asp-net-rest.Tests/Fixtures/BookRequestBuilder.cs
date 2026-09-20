using AspNetRest.Contracts;

namespace AspNetRest.Tests.Fixtures;

public sealed record BookRequestBuilder(string Title, string Author, int Year)
{
    public static BookRequestBuilder ABook() =>
        new(BookFixtures.NewTitle, BookFixtures.NewAuthor, BookFixtures.NewYear);

    public static BookRequestBuilder TheSeededBook() =>
        new(BookFixtures.SeededTitle, BookFixtures.SeededAuthor, BookFixtures.SeededYear);

    public BookRequestBuilder WithTitle(string title) => this with { Title = title };

    public BookRequestBuilder WithAuthor(string author) => this with { Author = author };

    public BookRequestBuilder WithYear(int year) => this with { Year = year };

    public CreateBookRequest AsCreateRequest() => new(Title, Author, Year);

    public UpdateBookRequest AsUpdateRequest() => new(Title, Author, Year);
}
