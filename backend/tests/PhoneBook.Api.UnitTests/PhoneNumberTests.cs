using Microsoft.Extensions.Time.Testing;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.UnitTests;

public class PhoneNumberTests
{
    private static readonly FakeTimeProvider Clock = new(TestData.Now);

    [Fact]
    public void Create_ValidInput_TrimsNameAndNormalizesNumber()
    {
        var entry = PhoneNumber.Create(
            "  Alice Anderson ",
            "+420 601 234 567",
            Visibility.Shared,
            TestData.Alice,
            "alice",
            Clock);

        Assert.Equal("Alice Anderson", entry.ContactName);
        Assert.Equal("+420601234567", entry.Number);
        Assert.Equal(Visibility.Shared, entry.Visibility);
        Assert.Equal(TestData.Alice, entry.OwnerId);
        Assert.Equal("alice", entry.OwnerUsername);
    }

    [Fact]
    public void Create_ValidInput_TakesCreatedAtFromTimeProvider()
    {
        var entry = TestData.Entry(TestData.Alice, Visibility.Personal);

        Assert.Equal(TestData.Now, entry.CreatedAt);
    }

    [Fact]
    public void Create_ValidInput_GeneratesTimeOrderedVersion7Id()
    {
        var clock = new FakeTimeProvider(TestData.Now);
        var first = PhoneNumber.Create("A", "+420601234567", Visibility.Personal, TestData.Alice, "alice", clock);
        clock.Advance(TimeSpan.FromSeconds(1));
        var second = PhoneNumber.Create("B", "+420601234567", Visibility.Personal, TestData.Alice, "alice", clock);

        Assert.Equal(7, first.Id.Version);
        Assert.True(first.Id.CompareTo(second.Id) < 0);
    }

    [Fact]
    public void Create_InvalidContactName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PhoneNumber.Create(" ", "+420601234567", Visibility.Personal, TestData.Alice, "alice", Clock));
    }

    [Fact]
    public void Create_InvalidNumber_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PhoneNumber.Create("Alice", "12", Visibility.Personal, TestData.Alice, "alice", Clock));
    }

    [Fact]
    public void Create_UndefinedVisibility_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PhoneNumber.Create("Alice", "+420601234567", (Visibility)42, TestData.Alice, "alice", Clock));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyOwnerId_Throws(string ownerId)
    {
        Assert.Throws<ArgumentException>(() =>
            PhoneNumber.Create("Alice", "+420601234567", Visibility.Personal, ownerId, "alice", Clock));
    }
}
