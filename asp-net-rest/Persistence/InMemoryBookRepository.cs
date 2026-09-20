using AspNetRest.Domain;

namespace AspNetRest.Persistence;

public sealed class InMemoryBookRepository : IBookRepository
{
    private readonly Lock _lock = new();
    private readonly Dictionary<int, Book> _books = [];
    private int _lastId;

    public InMemoryBookRepository() =>
        Seed("Der Steppenwolf", "Hermann Hesse", 1927)
            .Seed("Die Physiker", "Friedrich Dürrenmatt", 1962)
            .Seed("Homo Faber", "Max Frisch", 1957);

    public IReadOnlyList<Book> GetAll()
    {
        lock (_lock)
        {
            return _books.Values.OrderBy(GetId).ToList();
        }
    }

    public Book? GetById(int id)
    {
        lock (_lock)
        {
            return _books.GetValueOrDefault(id);
        }
    }

    public bool ExistsSameEdition(string title, string author, int? ignoredId)
    {
        lock (_lock)
        {
            return _books.Values.Any(book => IsSameEdition(book, title, author, ignoredId));
        }
    }

    public Book Add(string title, string author, int year)
    {
        lock (_lock)
        {
            return Insert(title, author, year);
        }
    }

    public Book? Replace(int id, string title, string author, int year)
    {
        lock (_lock)
        {
            if (!_books.ContainsKey(id))
            {
                return null;
            }

            var replaced = new Book(id, title, author, year);
            _books[id] = replaced;
            return replaced;
        }
    }

    public Book? Patch(int id, string? title, string? author, int? year)
    {
        lock (_lock)
        {
            if (!_books.TryGetValue(id, out var existing))
            {
                return null;
            }

            var patched = existing with
            {
                Title = title ?? existing.Title,
                Author = author ?? existing.Author,
                Year = year ?? existing.Year
            };

            _books[id] = patched;
            return patched;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            return _books.Remove(id);
        }
    }

    private InMemoryBookRepository Seed(string title, string author, int year)
    {
        Insert(title, author, year);

        return this;
    }

    private Book Insert(string title, string author, int year)
    {
        var book = new Book(++_lastId, title, author, year);
        _books[book.Id] = book;
        return book;
    }

    private static int GetId(Book book) => book.Id;

    private static bool IsSameEdition(Book book, string title, string author, int? ignoredId) =>
        book.Id != ignoredId
        && string.Equals(book.Title, title, StringComparison.OrdinalIgnoreCase)
        && string.Equals(book.Author, author, StringComparison.OrdinalIgnoreCase);
}
