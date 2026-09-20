using System.Net;
using System.Net.Http.Json;
using AspNetRest.Contracts;
using AspNetRest.Services;
using AspNetRest.Tests.Fixtures;
using AspNetRest.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using static AspNetRest.Tests.Fixtures.BookRequestBuilder;
using static AspNetRest.Tests.Fixtures.PatchRequestBuilder;

namespace AspNetRest.Tests.Api;

public sealed class BookEndpointsTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public BookEndpointsTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task WhenListingBooks_ThenTheSeededPageIsReturned()
    {
        var page = await _client.GetFromJsonAsync<BookPageResponse>("/api/books");

        page.Should().BeEquivalentTo(new
        {
            TotalCount = BookFixtures.SeededBookCount,
            SortedBy = SortKeys.ById
        });
    }

    [Fact]
    public async Task GivenSortByTitle_WhenListingBooks_ThenTheOrderFollowsTheTitles()
    {
        var page = await _client.GetFromJsonAsync<BookPageResponse>("/api/books?sortBy=title");

        page.Should().NotBeNull()
            .And.Subject.As<BookPageResponse>()
            .Items[0].Title.Should().Be(BookFixtures.SeededTitle);
    }

    [Fact]
    public async Task GivenInvalidQueryParameters_WhenListingBooks_ThenValidationProblemsAreReturned()
    {
        var response = await _client.GetAsync("/api/books?sortBy=verlag&page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem.Errors.Keys.Should()
            .Contain(ValidationMessages.SortByField)
            .And.Contain(ValidationMessages.PageField);
    }

    [Fact]
    public async Task GivenAKnownId_WhenReadingABook_ThenTheBookIsReturned()
    {
        var book = await _client.GetFromJsonAsync<BookResponse>(BookUri(BookFixtures.FirstBookId));

        book.Should().BeEquivalentTo(new
        {
            Id = BookFixtures.FirstBookId,
            Title = BookFixtures.SeededTitle
        });
    }

    [Fact]
    public async Task GivenUnknownId_WhenReadingABook_ThenNotFoundIsReturned()
    {
        var response = await _client.GetAsync(BookUri(BookFixtures.UnknownBookId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GivenAKnownId_WhenSendingHead_ThenOkWithoutBodyIsReturned()
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, BookUri(BookFixtures.FirstBookId));

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task GivenUnknownId_WhenSendingHead_ThenNotFoundIsReturned()
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, BookUri(BookFixtures.UnknownBookId));

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GivenAValidBook_WhenCreatingIt_ThenCreatedWithLocationIsReturned()
    {
        var request = ABook().AsCreateRequest();

        var response = await _client.PostAsJsonAsync("/api/books", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var expectedId = BookFixtures.SeededBookCount + 1;
        response.Headers.Location?.ToString().Should().Be(BookUri(expectedId));

        var created = await response.Content.ReadFromJsonAsync<BookResponse>();
        created.Should().BeEquivalentTo(new { Id = expectedId });
    }

    [Fact]
    public async Task GivenAnEmptyBook_WhenCreatingIt_ThenEveryFieldIsReported()
    {
        var request = ABook().WithTitle(string.Empty).WithAuthor(string.Empty).WithYear(0).AsCreateRequest();

        var response = await _client.PostAsJsonAsync("/api/books", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem.Errors.Keys.Should()
            .Contain(ValidationMessages.TitleField)
            .And.Contain(ValidationMessages.AuthorField)
            .And.Contain(ValidationMessages.YearField);
    }

    [Fact]
    public async Task GivenAnAlreadyStoredEdition_WhenCreatingItAgain_ThenTheDuplicateIsRejected()
    {
        var request = TheSeededBook().AsCreateRequest();

        var response = await _client.PostAsJsonAsync("/api/books", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GivenAKnownId_WhenReplacingABook_ThenTheNewVersionIsReturned()
    {
        var request = ABook().WithTitle(BookFixtures.OtherTitle).WithYear(BookFixtures.OtherYear).AsUpdateRequest();

        var response = await _client.PutAsJsonAsync(BookUri(BookFixtures.FirstBookId), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var replaced = await response.Content.ReadFromJsonAsync<BookResponse>();
        replaced.Should().BeEquivalentTo(new { Title = BookFixtures.OtherTitle });
    }

    [Fact]
    public async Task GivenUnknownId_WhenReplacingABook_ThenNotFoundIsReturned()
    {
        var request = ABook().WithTitle(BookFixtures.OtherTitle).WithYear(BookFixtures.OtherYear).AsUpdateRequest();

        var response = await _client.PutAsJsonAsync(BookUri(BookFixtures.UnknownBookId), request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GivenOnlyOneChangedField_WhenPatchingABook_ThenTheOtherFieldsRemain()
    {
        var request = APatch().WithYear(BookFixtures.OtherYear).Build();

        var response = await _client.PatchAsJsonAsync(BookUri(BookFixtures.FirstBookId), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var patched = await response.Content.ReadFromJsonAsync<BookResponse>();
        patched.Should().BeEquivalentTo(new
        {
            Title = BookFixtures.SeededTitle,
            Year = BookFixtures.OtherYear
        });
    }

    [Fact]
    public async Task GivenAnEmptyPatch_WhenPatchingABook_ThenAChangeIsDemanded()
    {
        var request = APatch().Build();

        var response = await _client.PatchAsJsonAsync(BookUri(BookFixtures.FirstBookId), request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem.Errors.Keys.Should().Contain(ValidationMessages.RequestField);
    }

    [Fact]
    public async Task GivenAKnownId_WhenDeletingTwice_ThenNoContentAndThenNotFoundIsReturned()
    {
        var firstResponse = await _client.DeleteAsync(BookUri(BookFixtures.FirstBookId));
        var secondResponse = await _client.DeleteAsync(BookUri(BookFixtures.FirstBookId));

        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenAskingForOptions_ThenTheAllowedMethodsAreListed()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/books");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var allowed = string.Join(string.Empty, response.Content.Headers.GetValues("Allow"));
        allowed.Should()
            .Contain(HttpMethods.Patch)
            .And.Contain(HttpMethods.Delete);
    }

    private static string BookUri(int id) => $"/api/books/{id}";

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
