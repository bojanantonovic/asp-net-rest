using AspNetRest.Options;
using FluentValidation;

namespace AspNetRest.Validation;

public sealed class BookLibraryOptionsValidator : AbstractValidator<BookLibraryOptions>
{
    private const int Zero = 0;

    public BookLibraryOptionsValidator()
    {
        RuleFor(options => options.MaxTitleLength)
            .GreaterThan(Zero)
            .WithMessage(ValidationMessages.OptionMustBePositive);

        RuleFor(options => options.MaxAuthorLength)
            .GreaterThan(Zero)
            .WithMessage(ValidationMessages.OptionMustBePositive);

        RuleFor(options => options.EarliestPublicationYear)
            .GreaterThan(Zero)
            .WithMessage(ValidationMessages.OptionMustBePositive);

        RuleFor(options => options.MaxPageSize)
            .GreaterThan(Zero)
            .WithMessage(ValidationMessages.OptionMustBePositive);

        RuleFor(options => options.DefaultPageSize)
            .GreaterThan(Zero)
            .WithMessage(ValidationMessages.OptionMustBePositive)
            .LessThanOrEqualTo(options => options.MaxPageSize)
            .WithMessage(ValidationMessages.DefaultPageSizeTooLarge);
    }
}
