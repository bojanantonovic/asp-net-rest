using AspNetRest.Contracts;
using AspNetRest.Options;
using AspNetRest.Services;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AspNetRest.Validation;

public sealed class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    private const int NextYearOffset = 1;

    public UpdateBookRequestValidator(IClock clock, IOptions<BookLibraryOptions> options)
    {
        var limits = options.Value;

        RuleFor(request => request.Title).BookTitle(limits.MaxTitleLength);
        RuleFor(request => request.Author).AuthorName(limits.MaxAuthorLength);
        RuleFor(request => request.Year)
            .PublicationYear(limits.EarliestPublicationYear, clock.UtcNow.Year + NextYearOffset);
    }
}
