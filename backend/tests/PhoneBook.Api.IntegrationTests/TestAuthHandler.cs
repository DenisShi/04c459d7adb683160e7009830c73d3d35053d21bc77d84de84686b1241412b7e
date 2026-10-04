using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhoneBook.Api.Infrastructure.Auth;

namespace PhoneBook.Api.IntegrationTests;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string SubjectHeader = "X-Test-Sub";
    public const string UsernameHeader = "X-Test-Username";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var subject = Request.Headers[SubjectHeader].ToString();
        if (subject.Length == 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new(CurrentUser.SubjectClaim, subject) };
        var username = Request.Headers[UsernameHeader].ToString();
        if (username.Length > 0)
        {
            claims.Add(new Claim(CurrentUser.UsernameClaim, username));
        }

        var identity = new ClaimsIdentity(claims, SchemeName, CurrentUser.UsernameClaim, roleType: null);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
