using AspNetRest.Domain;

namespace AspNetRest.Persistence;

public sealed class LoggingBookRepository(IBookRepository inner, ILogger<LoggingBookRepository> logger)
    : IBookRepository
{
    private const string AddedMessage = "Book created: {BookId} - {Title}";
    private const string ReplacedMessage = "Book replaced: {BookId}";
    private const string PatchedMessage = "Book patched: {BookId}";
    private const string DeletedMessage = "Book deleted: {BookId} (succeeded: {Deleted})";

    public IReadOnlyList<Book> GetAll() => inner.GetAll();

    public Book? GetById(int id) => inner.GetById(id);

    public bool ExistsSameEdition(string title, string author, int? ignoredId) =>
        inner.ExistsSameEdition(title, author, ignoredId);

    public Book Add(string title, string author, int year)
    {
        var created = inner.Add(title, author, year);
        logger.LogInformation(AddedMessage, created.Id, created.Title);
        return created;
    }

    public Book? Replace(int id, string title, string author, int year)
    {
        var replaced = inner.Replace(id, title, author, year);
        if (replaced is not null)
        {
            logger.LogInformation(ReplacedMessage, id);
        }

        return replaced;
    }

    public Book? Patch(int id, string? title, string? author, int? year)
    {
        var patched = inner.Patch(id, title, author, year);
        if (patched is not null)
        {
            logger.LogInformation(PatchedMessage, id);
        }

        return patched;
    }

    public bool Delete(int id)
    {
        var deleted = inner.Delete(id);
        logger.LogInformation(DeletedMessage, id, deleted);
        return deleted;
    }
}
