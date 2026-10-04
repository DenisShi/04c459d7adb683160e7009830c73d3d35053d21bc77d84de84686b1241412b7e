using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using PhoneBook.Api.Infrastructure.Auth;

namespace PhoneBook.Api.IntegrationTests;

public sealed class TestTokens : IDisposable
{
    public const string Issuer = "https://keycloak.test/realms/phonebook";
    public const string Audience = "phonebook-api";
    public const string KeyId = "test-signing-key";

    private readonly RSA rsa = RSA.Create(2048);
    private readonly JsonWebTokenHandler handler = new();

    public TestTokens()
    {
        SigningKey = new RsaSecurityKey(rsa) { KeyId = KeyId };
        Configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
        Configuration.SigningKeys.Add(SigningKey);
    }

    public RsaSecurityKey SigningKey { get; }

    public OpenIdConnectConfiguration Configuration { get; }

    public string Create(string? subject, string? username, Action<SecurityTokenDescriptor>? customize = null)
    {
        var now = DateTime.UtcNow;
        var claims = new Dictionary<string, object> { ["typ"] = "Bearer" };
        if (subject is not null)
        {
            claims[CurrentUser.SubjectClaim] = subject;
        }

        if (username is not null)
        {
            claims[CurrentUser.UsernameClaim] = username;
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(5),
            Claims = claims,
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.RsaSha256)
        };
        customize?.Invoke(descriptor);
        return handler.CreateToken(descriptor);
    }

    public void Dispose() => rsa.Dispose();
}
