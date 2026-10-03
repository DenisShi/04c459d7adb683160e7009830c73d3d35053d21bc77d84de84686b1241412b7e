using PhoneBook.Api.Application;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.UnitTests;

public class PhoneNumberQueriesTests
{
    private static readonly PhoneNumber AlicePersonal = TestData.Entry(TestData.Alice, Visibility.Personal);
    private static readonly PhoneNumber AliceShared = TestData.Entry(TestData.Alice, Visibility.Shared);
    private static readonly PhoneNumber BobPersonal = TestData.Entry(TestData.Bob, Visibility.Personal);
    private static readonly PhoneNumber BobShared = TestData.Entry(TestData.Bob, Visibility.Shared);

    private static readonly IQueryable<PhoneNumber> All =
        new[] { AlicePersonal, AliceShared, BobPersonal, BobShared }.AsQueryable();

    [Fact]
    public void VisibleIn_AllScope_ReturnsOwnEntriesAndSharedEntries()
    {
        var result = All.VisibleIn(PhoneNumberScope.All, TestData.Alice).ToList();

        Assert.Equal(3, result.Count);
        Assert.Contains(AlicePersonal, result);
        Assert.Contains(AliceShared, result);
        Assert.Contains(BobShared, result);
        Assert.DoesNotContain(BobPersonal, result);
    }

    [Fact]
    public void VisibleIn_PersonalScope_ReturnsOnlyOwnPersonalEntries()
    {
        var result = All.VisibleIn(PhoneNumberScope.Personal, TestData.Alice).ToList();

        Assert.Equal([AlicePersonal], result);
    }

    [Fact]
    public void VisibleIn_SharedScope_ReturnsSharedEntriesIncludingOwn()
    {
        var result = All.VisibleIn(PhoneNumberScope.Shared, TestData.Alice).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(AliceShared, result);
        Assert.Contains(BobShared, result);
    }

    [Theory]
    [InlineData(PhoneNumberScope.All)]
    [InlineData(PhoneNumberScope.Personal)]
    [InlineData(PhoneNumberScope.Shared)]
    public void VisibleIn_AnyScope_NeverReturnsPersonalEntriesOfAnotherUser(PhoneNumberScope scope)
    {
        var result = All.VisibleIn(scope, TestData.Alice).ToList();

        Assert.DoesNotContain(BobPersonal, result);
    }

    [Theory]
    [InlineData(PhoneNumberScope.All)]
    [InlineData(PhoneNumberScope.Personal)]
    [InlineData(PhoneNumberScope.Shared)]
    public void VisibleIn_UnknownSubject_ReturnsNoPersonalEntries(PhoneNumberScope scope)
    {
        var result = All.VisibleIn(scope, "someone-else").ToList();

        Assert.All(result, entry => Assert.Equal(Visibility.Shared, entry.Visibility));
    }

    [Fact]
    public void VisibleIn_UndefinedScope_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => All.VisibleIn((PhoneNumberScope)42, TestData.Alice));
    }
}
