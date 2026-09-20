namespace AspNetRest.Options;

public sealed class BookLibraryOptions
{
    public const string SectionName = "BookLibrary";

    public int EarliestPublicationYear { get; set; } = 1450;

    public int MaxTitleLength { get; set; } = 200;

    public int MaxAuthorLength { get; set; } = 100;

    public int DefaultPageSize { get; set; } = 20;

    public int MaxPageSize { get; set; } = 100;
}
