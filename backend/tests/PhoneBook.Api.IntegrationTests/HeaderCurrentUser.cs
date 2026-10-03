using Microsoft.AspNetCore.Http;
using PhoneBook.Api.Application;

namespace PhoneBook.Api.IntegrationTests;

public sealed class HeaderCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public const string SubjectHeader = "X-Test-Sub";
    public const string UsernameHeader = "X-Test-Username";

    public string Subject => Read(SubjectHeader);

    public string Username => Read(UsernameHeader);

    private string Read(string header) =>
        httpContextAccessor.HttpContext?.Request.Headers[header].ToString() is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"The test header '{header}' is missing.");
}
