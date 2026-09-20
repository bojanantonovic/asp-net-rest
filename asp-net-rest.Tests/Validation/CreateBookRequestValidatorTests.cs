using AspNetRest.Persistence;
using AspNetRest.Tests.Fixtures;
using AspNetRest.Validation;
using FluentValidation.Results;
using static AspNetRest.Tests.Fixtures.BookRequestBuilder;

namespace AspNetRest.Tests.Validation;

public sealed class CreateBookRequestValidatorTests
{
    private const int TitleLengthAboveLimit = 201;

    private readonly InMemoryBookRepository _repository = new();

    [Fact]
    public void GivenValidRequest_WhenValidating_ThenNoErrorsAreReported()
    {
        var validator = CreateValidator();
        var request = ABook().AsCreateRequest();

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenEmptyTitle_WhenValidating_ThenTitleIsRejected()
    {
        var validator = CreateValidator();
        var request = ABook().WithTitle(string.Empty).AsCreateRequest();

        var result = validator.Validate(request);

        ShouldReportOnly(result, ValidationMessages.TitleRequired);
    }

    [Fact]
    public void GivenTooLongTitle_WhenValidating_ThenTitleIsRejected()
    {
        var validator = CreateValidator();
        var request = ABook().WithTitle(new string('x', TitleLengthAboveLimit)).AsCreateRequest();

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public void GivenMissingAuthor_WhenValidating_ThenAuthorIsRejected()
    {
        var validator = CreateValidator();
        var request = ABook().WithAuthor("   ").AsCreateRequest();

        var result = validator.Validate(request);

        ShouldReportOnly(result, ValidationMessages.AuthorRequired);
    }

    [Theory]
    [InlineData(1449)]
    [InlineData(FixedClock.Year + 2)]
    public void GivenImplausibleYear_WhenValidating_ThenYearIsRejected(int year)
    {
        var validator = CreateValidator();
        var request = ABook().WithYear(year).AsCreateRequest();

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GivenYearInNextYear_WhenValidating_ThenAnnouncementIsAccepted()
    {
        var validator = CreateValidator();
        var request = ABook().WithYear(FixedClock.Year + 1).AsCreateRequest();

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenAlreadyStoredEdition_WhenValidating_ThenDuplicateIsRejected()
    {
        var validator = CreateValidator();
        var request = TheSeededBook()
            .WithTitle(BookFixtures.SeededTitle.ToUpperInvariant())
            .AsCreateRequest();

        var result = validator.Validate(request);

        ShouldReportOnly(result, ValidationMessages.DuplicateEdition);
    }

    private CreateBookRequestValidator CreateValidator() =>
        new(_repository, FixedClock.Default, BookFixtures.Limits());

    private static void ShouldReportOnly(ValidationResult result, string expectedMessage) =>
        result.Errors.Should()
            .ContainSingle()
            .Which.ErrorMessage.Should().Be(expectedMessage);
}
