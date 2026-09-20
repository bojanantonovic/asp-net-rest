using AspNetRest.Contracts;
using AspNetRest.Options;
using AspNetRest.Services;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AspNetRest.Validation;

public sealed class BookPageQueryValidator : AbstractValidator<BookPageQuery>
{
    private const int FirstPage = 1;
    private const int SmallestPageSize = 1;

    public BookPageQueryValidator(IOptions<BookLibraryOptions> options)
    {
        var limits = options.Value;

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(FirstPage)
            .WithMessage(ValidationMessages.PageTooSmall)
            .OverridePropertyName(ValidationMessages.PageField)
            .When(HasPage);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(SmallestPageSize, limits.MaxPageSize)
            .WithMessage(ValidationMessages.PageSizeOutOfRange)
            .OverridePropertyName(ValidationMessages.PageSizeField)
            .When(HasPageSize);

        RuleFor(query => query.SortBy)
            .Must(BeAKnownSortKey)
            .WithMessage(BuildUnknownSortKeyMessage())
            .OverridePropertyName(ValidationMessages.SortByField)
            .When(HasSortBy);
    }

    private static bool HasPage(BookPageQuery query) => query.Page.HasValue;

    private static bool HasPageSize(BookPageQuery query) => query.PageSize.HasValue;

    private static bool HasSortBy(BookPageQuery query) => query.SortBy is not null;

    private static bool BeAKnownSortKey(string? sortBy) => SortKeys.IsKnown(sortBy);

    private static string BuildUnknownSortKeyMessage() =>
        ValidationMessages.UnknownSortKey.Replace(
            ValidationMessages.AllowedKeysPlaceholder,
            string.Join(", ", SortKeys.All));
}
