using PhoneBook.Api.Domain;

namespace PhoneBook.Api.UnitTests;

public class VisibilityRulesTests
{
    [Theory]
    [InlineData(TestData.Alice, Visibility.Personal, true)]
    [InlineData(TestData.Alice, Visibility.Shared, true)]
    [InlineData(TestData.Bob, Visibility.Personal, false)]
    [InlineData(TestData.Bob, Visibility.Shared, true)]
    public void VisibleTo_EntryOfOwner_FollowsVisibilityRule(string ownerId, Visibility visibility, bool expected)
    {
        var entry = TestData.Entry(ownerId, visibility);

        var visible = VisibilityRules.VisibleTo(TestData.Alice).Compile()(entry);

        Assert.Equal(expected, visible);
    }

    [Fact]
    public void VisibleTo_PersonalEntryOfAnotherUser_ReturnsFalse()
    {
        var entry = TestData.Entry(TestData.Bob, Visibility.Personal);

        Assert.False(VisibilityRules.VisibleTo(TestData.Alice).Compile()(entry));
    }

    [Theory]
    [InlineData(TestData.Alice, Visibility.Personal, true)]
    [InlineData(TestData.Alice, Visibility.Shared, false)]
    [InlineData(TestData.Bob, Visibility.Personal, false)]
    [InlineData(TestData.Bob, Visibility.Shared, false)]
    public void PersonalOf_Entry_MatchesOnlyPersonalEntriesOfSubject(string ownerId, Visibility visibility, bool expected)
    {
        var entry = TestData.Entry(ownerId, visibility);

        Assert.Equal(expected, VisibilityRules.PersonalOf(TestData.Alice).Compile()(entry));
    }

    [Theory]
    [InlineData(TestData.Alice, Visibility.Personal, false)]
    [InlineData(TestData.Alice, Visibility.Shared, true)]
    [InlineData(TestData.Bob, Visibility.Personal, false)]
    [InlineData(TestData.Bob, Visibility.Shared, true)]
    public void SharedWithEveryone_Entry_MatchesOnlySharedEntries(string ownerId, Visibility visibility, bool expected)
    {
        var entry = TestData.Entry(ownerId, visibility);

        Assert.Equal(expected, VisibilityRules.SharedWithEveryone().Compile()(entry));
    }
}
