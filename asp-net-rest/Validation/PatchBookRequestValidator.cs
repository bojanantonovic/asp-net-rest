using AspNetRest.Contracts;
using AspNetRest.Options;
using AspNetRest.Services;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AspNetRest.Validation;

public sealed class PatchBookRequestValidator : AbstractValidator<PatchBookRequest>
{
    private const int NextYearOffset = 1;

    public PatchBookRequestValidator(IClock clock, IOptions<BookLibraryOptions> options)
    {
        var limits = options.Value;

        RuleFor(request => request.Title!)
            .BookTitle(limits.MaxTitleLength)
            .When(HasTitle);

        RuleFor(request => request.Author!)
            .AuthorName(limits.MaxAuthorLength)
            .When(HasAuthor);

        RuleFor(request => request.Year)
            .PublicationYear(limits.EarliestPublicationYear, clock.UtcNow.Year + NextYearOffset)
            .When(HasYear);

        RuleFor(request => request)
            .Must(ChangeAtLeastOneField)
            .WithMessage(ValidationMessages.PatchWithoutChange)
            .OverridePropertyName(ValidationMessages.RequestField);
    }

    private static bool HasTitle(PatchBookRequest request) => request.Title is not null;

    private static bool HasAuthor(PatchBookRequest request) => request.Author is not null;

    private static bool HasYear(PatchBookRequest request) => request.Year is not null;

    private static bool ChangeAtLeastOneField(PatchBookRequest request) =>
        HasTitle(request) || HasAuthor(request) || HasYear(request);
}
