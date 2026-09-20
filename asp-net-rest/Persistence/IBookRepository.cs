using AspNetRest.Domain;

namespace AspNetRest.Persistence;

public interface IBookRepository
{
    IReadOnlyList<Book> GetAll();

    Book? GetById(int id);

    bool ExistsSameEdition(string title, string author, int? ignoredId);

    Book Add(string title, string author, int year);

    Book? Replace(int id, string title, string author, int year);

    Book? Patch(int id, string? title, string? author, int? year);

    bool Delete(int id);
}
