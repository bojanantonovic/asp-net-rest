using AspNetRest.Contracts;
using AspNetRest.Domain;
using AspNetRest.Options;
using AspNetRest.Persistence;
using Microsoft.Extensions.Options;

namespace AspNetRest.Services;

public sealed class BookService(
    IBookRepository repository,
    IBookMapper mapper,
    BookSorterSelector sorterSelector,
    IAuditTrail auditTrail,
    IOptions<BookLibraryOptions> options) : IBookService
{
    private const int FirstPage = 1;
    private const string ListedStep = "Liste gelesen: Seite {0}, Sortierung {1}";
    private const string ReadStep = "Buch {0} gelesen (gefunden: {1})";
    private const string CreatedStep = "Buch {0} angelegt";
    private const string ReplacedStep = "Buch {0} ersetzt (gefunden: {1})";
    private const string PatchedStep = "Buch {0} geändert (gefunden: {1})";
    private const string DeletedStep = "Buch {0} gelöscht (erfolgreich: {1})";
    private const string Yes = "ja";
    private const string No = "nein";

    private readonly BookLibraryOptions _options = options.Value;

    public BookPageResponse GetPage(BookPageQuery query)
    {
        var page = query.Page ?? FirstPage;
        var pageSize = ClampPageSize(query.PageSize);
        var sortKey = SortKeys.Normalize(query.SortBy);

        var sorted = sorterSelector.Select(sortKey).Sort(repository.GetAll());

        auditTrail.Record(string.Format(ListedStep, page, sortKey));

        return new BookPageResponse(
            mapper.ToResponses(sorted.Skip((page - FirstPage) * pageSize).Take(pageSize)),
            page,
            pageSize,
            sorted.Count,
            sortKey);
    }

    public BookResponse? Get(int id)
    {
        var book = repository.GetById(id);
        auditTrail.Record(string.Format(ReadStep, id, Describe(book is not null)));

        return MapOrNull(book);
    }

    public bool Exists(int id) => repository.GetById(id) is not null;

    public BookResponse Create(CreateBookRequest request)
    {
        var created = repository.Add(Normalize(request.Title), Normalize(request.Author), request.Year);
        auditTrail.Record(string.Format(CreatedStep, created.Id));

        return mapper.ToResponse(created);
    }

    public BookResponse? Replace(int id, UpdateBookRequest request)
    {
        var replaced = repository.Replace(id, Normalize(request.Title), Normalize(request.Author), request.Year);
        auditTrail.Record(string.Format(ReplacedStep, id, Describe(replaced is not null)));

        return MapOrNull(replaced);
    }

    public BookResponse? Patch(int id, PatchBookRequest request)
    {
        var patched = repository.Patch(id, request.Title?.Trim(), request.Author?.Trim(), request.Year);
        auditTrail.Record(string.Format(PatchedStep, id, Describe(patched is not null)));

        return MapOrNull(patched);
    }

    public bool Delete(int id)
    {
        var deleted = repository.Delete(id);
        auditTrail.Record(string.Format(DeletedStep, id, Describe(deleted)));

        return deleted;
    }

    private BookResponse? MapOrNull(Book? book) => book is null ? null : mapper.ToResponse(book);

    private static string Describe(bool value) => value ? Yes : No;

    private int ClampPageSize(int? requested) =>
        requested is null ? _options.DefaultPageSize : Math.Clamp(requested.Value, 1, _options.MaxPageSize);

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
}
