using AspNetRest.Contracts;
using AspNetRest.Domain;

namespace AspNetRest.Services;

public interface IBookMapper
{
    BookResponse ToResponse(Book book);

    IReadOnlyList<BookResponse> ToResponses(IEnumerable<Book> books);
}

public sealed class BookMapper : IBookMapper
{
    public BookResponse ToResponse(Book book) => new(book.Id, book.Title, book.Author, book.Year);

    public IReadOnlyList<BookResponse> ToResponses(IEnumerable<Book> books) =>
        books.Select(ToResponse).ToList();
}
