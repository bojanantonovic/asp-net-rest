using AspNetRest.Contracts;
using AspNetRest.Options;
using AspNetRest.Persistence;
using AspNetRest.Services;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AspNetRest.Validation;

public sealed class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    private const int NextYearOffset = 1;

    private readonly IBookRepository _repository;

    public CreateBookRequestValidator(
        IBookRepository repository,
        IClock clock,
        IOptions<BookLibraryOptions> options)
    {
        _repository = repository;
        var limits = options.Value;

        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Title)
            .BookTitle(limits.MaxTitleLength);

        RuleFor(request => request.Author)
            .AuthorName(limits.MaxAuthorLength);

        RuleFor(request => request.Year)
            .PublicationYear(limits.EarliestPublicationYear, clock.UtcNow.Year + NextYearOffset);

        RuleFor(request => request)
            .Must(BeUniqueEdition)
            .WithMessage(ValidationMessages.DuplicateEdition)
            .OverridePropertyName(ValidationMessages.TitleField)
            .When(HasTitleAndAuthor);
    }

    private bool BeUniqueEdition(CreateBookRequest request) =>
        !_repository.ExistsSameEdition(request.Title.Trim(), request.Author.Trim(), ignoredId: null);

    private static bool HasTitleAndAuthor(CreateBookRequest request) =>
        !string.IsNullOrWhiteSpace(request.Title) && !string.IsNullOrWhiteSpace(request.Author);
}
