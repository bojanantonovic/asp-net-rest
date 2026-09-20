using FluentValidation;

namespace AspNetRest.Validation;

public static class BookRuleExtensions
{
    public static IRuleBuilderOptions<T, string> BookTitle<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int maxLength) =>
        ruleBuilder
            .NotEmpty().WithMessage(ValidationMessages.TitleRequired)
            .MaximumLength(maxLength).WithMessage(ValidationMessages.TitleTooLong);

    public static IRuleBuilderOptions<T, string> AuthorName<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int maxLength) =>
        ruleBuilder
            .NotEmpty().WithMessage(ValidationMessages.AuthorRequired)
            .MaximumLength(maxLength).WithMessage(ValidationMessages.AuthorTooLong);

    public static IRuleBuilderOptions<T, int> PublicationYear<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        int earliestYear,
        int latestYear) =>
        ruleBuilder
            .InclusiveBetween(earliestYear, latestYear).WithMessage(ValidationMessages.YearOutOfRange);

    public static IRuleBuilderOptions<T, int?> PublicationYear<T>(
        this IRuleBuilder<T, int?> ruleBuilder,
        int earliestYear,
        int latestYear) =>
        ruleBuilder
            .InclusiveBetween(earliestYear, latestYear).WithMessage(ValidationMessages.YearOutOfRange);
}
