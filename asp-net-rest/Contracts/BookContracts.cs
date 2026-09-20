namespace AspNetRest.Contracts;

public sealed record CreateBookRequest(string Title, string Author, int Year);

public sealed record UpdateBookRequest(string Title, string Author, int Year);

public sealed record PatchBookRequest(string? Title, string? Author, int? Year);

public sealed record BookResponse(int Id, string Title, string Author, int Year);

public sealed record BookPageResponse(
    IReadOnlyList<BookResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string SortedBy);

public sealed record BookPageQuery(int? Page, int? PageSize, string? SortBy);
