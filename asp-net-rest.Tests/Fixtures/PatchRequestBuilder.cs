using AspNetRest.Contracts;

namespace AspNetRest.Tests.Fixtures;

public sealed record PatchRequestBuilder(string? Title, string? Author, int? Year)
{
    public static PatchRequestBuilder APatch() => new(null, null, null);

    public PatchRequestBuilder WithTitle(string? title) => this with { Title = title };

    public PatchRequestBuilder WithAuthor(string? author) => this with { Author = author };

    public PatchRequestBuilder WithYear(int year) => this with { Year = year };

    public PatchBookRequest Build() => new(Title, Author, Year);
}
