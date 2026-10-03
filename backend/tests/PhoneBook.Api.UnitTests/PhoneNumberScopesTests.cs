using PhoneBook.Api.Application;

namespace PhoneBook.Api.UnitTests;

public class PhoneNumberScopesTests
{
    [Theory]
    [InlineData("all", PhoneNumberScope.All)]
    [InlineData("ALL", PhoneNumberScope.All)]
    [InlineData("personal", PhoneNumberScope.Personal)]
    [InlineData("Personal", PhoneNumberScope.Personal)]
    [InlineData("shared", PhoneNumberScope.Shared)]
    [InlineData("SHARED", PhoneNumberScope.Shared)]
    public void TryParse_KnownValue_IgnoresCase(string value, PhoneNumberScope expected)
    {
        Assert.True(PhoneNumberScopes.TryParse(value, out var scope));
        Assert.Equal(expected, scope);
    }

    [Fact]
    public void TryParse_Null_DefaultsToAll()
    {
        Assert.True(PhoneNumberScopes.TryParse(null, out var scope));
        Assert.Equal(PhoneNumberScope.All, scope);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("everyone")]
    [InlineData("all,shared")]
    [InlineData("shared ")]
    public void TryParse_UnknownValue_ReturnsFalse(string value)
    {
        Assert.False(PhoneNumberScopes.TryParse(value, out _));
    }
}
