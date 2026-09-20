using AspNetRest.Contracts;
using AspNetRest.Options;
using AspNetRest.Services;
using AspNetRest.Tests.Fixtures;
using AspNetRest.Validation;

namespace AspNetRest.Tests.Validation;

public sealed class BookPageQueryValidatorTests
{
    private const string UnknownSortKey = "publisher";
    private const int PageSizeAboveLimit = 9999;
    private const int InvalidPage = 0;

    private readonly BookLibraryOptions _limits = new();
    private readonly BookPageQueryValidator _validator;

    public BookPageQueryValidatorTests() => _validator = new BookPageQueryValidator(BookFixtures.Limits(_limits));

    [Fact]
    public void GivenNoParameters_WhenValidating_ThenDefaultsAreAccepted()
    {
        var query = new BookPageQuery(null, null, null);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(SortKeys.ById)]
    [InlineData(SortKeys.ByTitle)]
    [InlineData(SortKeys.ByYear)]
    public void GivenKnownSortKey_WhenValidating_ThenKeyIsAccepted(string sortKey)
    {
        var query = new BookPageQuery(null, null, sortKey);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenUnknownSortKey_WhenValidating_ThenAllowedKeysAreListed()
    {
        var query = new BookPageQuery(null, null, UnknownSortKey);

        var result = _validator.Validate(query);

        result.Errors.Should()
            .ContainSingle()
            .Which.ErrorMessage.Should()
            .Contain(UnknownSortKey)
            .And.Contain(SortKeys.ByTitle);
    }

    [Fact]
    public void GivenPageBelowOne_WhenValidating_ThenPageIsRejected()
    {
        var query = new BookPageQuery(InvalidPage, null, null);

        var result = _validator.Validate(query);

        result.Errors.Should()
            .ContainSingle()
            .Which.ErrorMessage.Should().Be(ValidationMessages.PageTooSmall);
    }

    [Fact]
    public void GivenPageSizeAboveMaximum_WhenValidating_ThenPageSizeIsRejected()
    {
        var query = new BookPageQuery(null, PageSizeAboveLimit, null);

        var result = _validator.Validate(query);

        result.Errors.Should()
            .ContainSingle()
            .Which.ErrorMessage.Should().Contain(_limits.MaxPageSize.ToString());
    }
}
