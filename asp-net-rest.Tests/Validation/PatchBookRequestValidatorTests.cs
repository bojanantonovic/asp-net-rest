using AspNetRest.Contracts;
using AspNetRest.Tests.Fixtures;
using AspNetRest.Validation;
using static AspNetRest.Tests.Fixtures.PatchRequestBuilder;

namespace AspNetRest.Tests.Validation;

public sealed class PatchBookRequestValidatorTests
{
    private readonly PatchBookRequestValidator _validator =
        new(FixedClock.Default, BookFixtures.Limits());

    [Fact]
    public void GivenOnlyYear_WhenValidating_ThenPatchIsAccepted()
    {
        var request = APatch().WithYear(BookFixtures.OtherYear).Build();

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenEmptyPatch_WhenValidating_ThenChangeIsDemanded()
    {
        var request = APatch().Build();

        var result = _validator.Validate(request);

        result.Errors.Should()
            .ContainSingle()
            .Which.ErrorMessage.Should().Be(ValidationMessages.PatchWithoutChange);
    }

    [Fact]
    public void GivenEmptyTitle_WhenValidating_ThenTitleIsRejected()
    {
        var request = APatch().WithTitle(string.Empty).Build();

        var result = _validator.Validate(request);

        result.Errors.Should()
            .ContainSingle()
            .Which.ErrorMessage.Should().Be(ValidationMessages.TitleRequired);
    }

    [Fact]
    public void GivenUntouchedFields_WhenValidating_ThenNullIsNotAnError()
    {
        var request = APatch().WithTitle(BookFixtures.OtherTitle).Build();

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
