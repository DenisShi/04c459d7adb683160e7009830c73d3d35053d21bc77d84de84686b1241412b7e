using System.Security.Claims;
using PhoneBook.Api.Application;

namespace PhoneBook.Api.Infrastructure.Auth;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public const string SubjectClaim = "sub";
    public const string UsernameClaim = "preferred_username";

    public string Subject => ReadClaim(SubjectClaim)
        ?? throw new InvalidOperationException("The authenticated principal has no 'sub' claim.");

    public string Username => ReadClaim(UsernameClaim) ?? Subject;

    private string? ReadClaim(string type)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirst(type)?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
