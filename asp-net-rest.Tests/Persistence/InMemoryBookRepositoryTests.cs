using AspNetRest.Persistence;
using AspNetRest.Tests.Fixtures;

namespace AspNetRest.Tests.Persistence;

public sealed class InMemoryBookRepositoryTests
{
    private readonly InMemoryBookRepository _repository = new();

    [Fact]
    public void WhenCreated_ThenSeedDataIsAvailable()
    {
        var books = _repository.GetAll();

        books.Count.Should().Be(BookFixtures.SeededBookCount);
    }

    [Fact]
    public void WhenAddingABook_ThenTheNextFreeIdIsAssigned()
    {
        var created = _repository.Add(BookFixtures.NewTitle, BookFixtures.NewAuthor, BookFixtures.NewYear);

        created.Id.Should().Be(BookFixtures.SeededBookCount + 1);
    }

    [Fact]
    public void GivenAStoredBook_WhenPatchingOneField_ThenTheOtherFieldsRemain()
    {
        var patched = _repository.Patch(
            BookFixtures.FirstBookId,
            title: null,
            author: null,
            year: BookFixtures.OtherYear);

        patched.Should().BeEquivalentTo(new
        {
            Title = BookFixtures.SeededTitle,
            Author = BookFixtures.SeededAuthor,
            Year = BookFixtures.OtherYear
        });
    }

    [Fact]
    public void GivenUnknownId_WhenReplacing_ThenNothingIsCreated()
    {
        var replaced = _repository.Replace(
            BookFixtures.UnknownBookId,
            BookFixtures.NewTitle,
            BookFixtures.NewAuthor,
            BookFixtures.NewYear);

        replaced.Should().BeNull();
        _repository.GetAll().Count.Should().Be(BookFixtures.SeededBookCount);
    }

    [Fact]
    public void GivenSameTitleAndAuthor_WhenCheckingForDuplicates_ThenCaseIsIgnored()
    {
        var exists = _repository.ExistsSameEdition(
            BookFixtures.SeededTitle.ToUpperInvariant(),
            BookFixtures.SeededAuthor.ToLowerInvariant(),
            ignoredId: null);

        exists.Should().BeTrue();
    }

    [Fact]
    public void GivenTheSameBook_WhenCheckingWithIgnoredId_ThenItIsNotItsOwnDuplicate()
    {
        var exists = _repository.ExistsSameEdition(
            BookFixtures.SeededTitle,
            BookFixtures.SeededAuthor,
            ignoredId: BookFixtures.FirstBookId);

        exists.Should().BeFalse();
    }

    [Fact]
    public void GivenAStoredBook_WhenDeletingTwice_ThenOnlyTheFirstAttemptSucceeds()
    {
        var firstAttempt = _repository.Delete(BookFixtures.FirstBookId);
        var secondAttempt = _repository.Delete(BookFixtures.FirstBookId);

        firstAttempt.Should().BeTrue();
        secondAttempt.Should().BeFalse();
    }
}
