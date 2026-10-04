using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PhoneBook.Api.Infrastructure.Auth;

public sealed class ConfigureJwtBearerOptions(IOptions<KeycloakOptions> keycloakOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public const string MissingSubjectMessage = "The access token has no 'sub' claim.";

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            Configure(options);
        }
    }

    public void Configure(JwtBearerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var keycloak = keycloakOptions.Value;
        options.MetadataAddress = keycloak.MetadataAddress;
        options.RequireHttpsMetadata = keycloak.RequireHttpsMetadata;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloak.ValidIssuer,
            ValidateAudience = true,
            ValidAudience = keycloak.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            NameClaimType = CurrentUser.UsernameClaim
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = RejectTokensWithoutSubject
        };
    }

    private static Task RejectTokensWithoutSubject(TokenValidatedContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Principal?.FindFirst(CurrentUser.SubjectClaim)?.Value))
        {
            context.Fail(MissingSubjectMessage);
        }

        return Task.CompletedTask;
    }
}
