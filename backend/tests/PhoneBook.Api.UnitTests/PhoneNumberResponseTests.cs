using PhoneBook.Api.Application;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.UnitTests;

public class PhoneNumberResponseTests
{
    [Fact]
    public void From_EntryOfCurrentUser_IsOwnedByCurrentUser()
    {
        var entry = TestData.Entry(TestData.Alice, Visibility.Personal, "alice");

        var response = PhoneNumberResponse.From(entry, TestData.Alice);

        Assert.True(response.IsOwnedByCurrentUser);
        Assert.Equal("alice", response.OwnerUsername);
    }

    [Fact]
    public void From_EntryOfAnotherUser_IsNotOwnedByCurrentUser()
    {
        var entry = TestData.Entry(TestData.Bob, Visibility.Shared, "bob");

        var response = PhoneNumberResponse.From(entry, TestData.Alice);

        Assert.False(response.IsOwnedByCurrentUser);
    }

    [Theory]
    [InlineData(TestData.Alice, true)]
    [InlineData(TestData.Bob, false)]
    public void ProjectionFor_Entry_MatchesFrom(string ownerId, bool expectedOwned)
    {
        var entry = TestData.Entry(ownerId, Visibility.Shared, "owner");

        var projected = PhoneNumberResponse.ProjectionFor(TestData.Alice).Compile()(entry);

        Assert.Equal(PhoneNumberResponse.From(entry, TestData.Alice), projected);
        Assert.Equal(expectedOwned, projected.IsOwnedByCurrentUser);
    }

    [Fact]
    public void Response_HasNoOwnerIdProperty()
    {
        var names = typeof(PhoneNumberResponse).GetProperties().Select(property => property.Name).ToList();

        Assert.DoesNotContain("OwnerId", names);
        Assert.Contains("OwnerUsername", names);
    }
}
