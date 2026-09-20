using AspNetRest.Contracts;
using AspNetRest.Services;
using AspNetRest.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using static AspNetRest.Tests.Fixtures.BookRequestBuilder;
using static AspNetRest.Tests.Fixtures.PatchRequestBuilder;

namespace AspNetRest.Tests.Services;

public sealed class BookServiceTests : IDisposable
{
    private const int PageSizeAboveLimit = 9999;
    private const int MaxPageSize = 100;
    private const int FirstPage = 1;

    private readonly ServiceProvider _services = TestHost.CreateServiceProvider();
    private readonly IServiceScope _scope;
    private readonly IBookService _books;

    public BookServiceTests()
    {
        _scope = _services.CreateScope();
        _books = _scope.ServiceProvider.GetRequiredService<IBookService>();
    }

    [Fact]
    public void WhenListingBooks_ThenSeedDataIsReturned()
    {
        var query = new BookPageQuery(null, null, null);

        var page = _books.GetPage(query);

        page.TotalCount.Should().Be(BookFixtures.SeededBookCount);
        page.SortedBy.Should().Be(SortKeys.ById);
    }

    [Fact]
    public void GivenSortByTitle_WhenListingBooks_ThenTheKeyedStrategyIsUsed()
    {
        var query = new BookPageQuery(null, null, SortKeys.ByTitle);

        var page = _books.GetPage(query);

        page.Items[0].Title.Should().Be(BookFixtures.SeededTitle);
    }

    [Fact]
    public void GivenPageSizeAboveMaximum_WhenListingBooks_ThenTheSizeIsClamped()
    {
        var query = new BookPageQuery(FirstPage, PageSizeAboveLimit, null);

        var page = _books.GetPage(query);

        page.PageSize.Should().Be(MaxPageSize);
    }

    [Fact]
    public void GivenASecondPage_WhenListingBooks_ThenOnlyTheRequestedWindowIsReturned()
    {
        var query = new BookPageQuery(2, 2, SortKeys.ById);

        var page = _books.GetPage(query);

        page.Items.Should().ContainSingle();
        page.TotalCount.Should().Be(BookFixtures.SeededBookCount);
    }

    [Fact]
    public void WhenCreatingABook_ThenItCanBeReadAgain()
    {
        var request = ABook().AsCreateRequest();

        var created = _books.Create(request);
        var reloaded = _books.Get(created.Id);

        reloaded.Should().BeEquivalentTo(new { Id = created.Id, Title = BookFixtures.NewTitle });
    }

    [Fact]
    public void GivenSurroundingWhitespace_WhenCreatingABook_ThenTheTitleIsTrimmed()
    {
        var request = ABook().WithTitle($"  {BookFixtures.NewTitle}  ").AsCreateRequest();

        var created = _books.Create(request);

        created.Title.Should().Be(BookFixtures.NewTitle);
    }

    [Fact]
    public void GivenUnknownId_WhenDeleting_ThenNothingIsReported()
    {
        var deleted = _books.Delete(BookFixtures.UnknownBookId);

        deleted.Should().BeFalse();
    }

    [Fact]
    public void GivenAPatchWithOnlyOneField_WhenPatching_ThenTheOtherFieldsRemain()
    {
        var request = APatch().WithYear(BookFixtures.OtherYear).Build();

        var patched = _books.Patch(BookFixtures.FirstBookId, request);

        patched.Should().BeEquivalentTo(new
        {
            Title = BookFixtures.SeededTitle,
            Year = BookFixtures.OtherYear
        });
    }

    [Fact]
    public void WhenUsingTheService_ThenTheAuditTrailOfTheSameScopeIsFilled()
    {
        var auditTrail = _scope.ServiceProvider.GetRequiredService<IAuditTrail>();

        _books.Get(BookFixtures.FirstBookId);

        auditTrail.Entries.Should().ContainSingle();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _services.Dispose();
    }
}
