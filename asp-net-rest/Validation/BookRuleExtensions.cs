using FluentValidation;

namespace AspNetRest.Validation;

public static class BookRuleExtensions
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> BookTitle(int maxLength) =>
            ruleBuilder
                .NotEmpty().WithMessage(ValidationMessages.TitleRequired)
                .MaximumLength(maxLength).WithMessage(ValidationMessages.TitleTooLong);

        public IRuleBuilderOptions<T, string> AuthorName(int maxLength) =>
            ruleBuilder
                .NotEmpty().WithMessage(ValidationMessages.AuthorRequired)
                .MaximumLength(maxLength).WithMessage(ValidationMessages.AuthorTooLong);
    }

    extension<T>(IRuleBuilder<T, int> ruleBuilder)
    {
        public IRuleBuilderOptions<T, int> PublicationYear(int earliestYear, int latestYear) =>
            ruleBuilder
                .InclusiveBetween(earliestYear, latestYear).WithMessage(ValidationMessages.YearOutOfRange);
    }

    extension<T>(IRuleBuilder<T, int?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, int?> PublicationYear(int earliestYear, int latestYear) =>
            ruleBuilder
                .InclusiveBetween(earliestYear, latestYear).WithMessage(ValidationMessages.YearOutOfRange);
    }
}
