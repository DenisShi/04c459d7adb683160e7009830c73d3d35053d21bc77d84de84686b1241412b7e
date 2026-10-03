using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PhoneBook.Api.Infrastructure.Auth;

namespace PhoneBook.Api.UnitTests;

public class CurrentUserTests
{
    private static CurrentUser CreateUser(params Claim[] claims)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
        };
        return new CurrentUser(new HttpContextAccessor { HttpContext = context });
    }

    [Fact]
    public void Subject_SubClaimPresent_ReturnsSubClaim()
    {
        var user = CreateUser(new Claim("sub", "alice-sub"));

        Assert.Equal("alice-sub", user.Subject);
    }

    [Fact]
    public void Username_PreferredUsernamePresent_ReturnsPreferredUsername()
    {
        var user = CreateUser(new Claim("sub", "alice-sub"), new Claim("preferred_username", "alice"));

        Assert.Equal("alice", user.Username);
    }

    [Fact]
    public void Username_PreferredUsernameMissing_FallsBackToSubject()
    {
        var user = CreateUser(new Claim("sub", "alice-sub"));

        Assert.Equal("alice-sub", user.Username);
    }

    [Fact]
    public void Username_PreferredUsernameBlank_FallsBackToSubject()
    {
        var user = CreateUser(new Claim("sub", "alice-sub"), new Claim("preferred_username", " "));

        Assert.Equal("alice-sub", user.Username);
    }

    [Fact]
    public void Subject_SubClaimMissing_Throws()
    {
        var user = CreateUser(new Claim("preferred_username", "alice"));

        Assert.Throws<InvalidOperationException>(() => user.Subject);
    }

    [Fact]
    public void Subject_SubClaimBlank_Throws()
    {
        var user = CreateUser(new Claim("sub", ""));

        Assert.Throws<InvalidOperationException>(() => user.Subject);
    }

    [Fact]
    public void Subject_NoHttpContext_Throws()
    {
        var user = new CurrentUser(new HttpContextAccessor());

        Assert.Throws<InvalidOperationException>(() => user.Subject);
    }

    [Fact]
    public void Subject_OnlyOtherClaimTypes_IgnoresThem()
    {
        var user = CreateUser(new Claim(ClaimTypes.NameIdentifier, "mapped-sub"));

        Assert.Throws<InvalidOperationException>(() => user.Subject);
    }
}
