using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PhoneBook.Api.Infrastructure.Auth;

namespace PhoneBook.Api.UnitTests;

public class ConfigureJwtBearerOptionsTests
{
    private static readonly KeycloakOptions Keycloak = new()
    {
        MetadataAddress = "http://keycloak:8080/realms/phonebook/.well-known/openid-configuration",
        ValidIssuer = "http://localhost:8080/realms/phonebook",
        Audience = "phonebook-api",
        RequireHttpsMetadata = false
    };

    private static JwtBearerOptions ConfigureDefaultScheme()
    {
        var options = new JwtBearerOptions();
        new ConfigureJwtBearerOptions(Options.Create(Keycloak)).Configure(JwtBearerDefaults.AuthenticationScheme, options);
        return options;
    }

    private static async Task<TokenValidatedContext> RaiseTokenValidatedAsync(params Claim[] claims)
    {
        var options = ConfigureDefaultScheme();
        var scheme = new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, null, typeof(JwtBearerHandler));
        var context = new TokenValidatedContext(new DefaultHttpContext(), scheme, options)
        {
            Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme))
        };
        await options.Events.TokenValidated(context);
        return context;
    }

    [Fact]
    public void Configure_DefaultScheme_UsesMetadataAddressAndHttpsRequirementFromKeycloakOptions()
    {
        var options = ConfigureDefaultScheme();

        Assert.Equal(Keycloak.MetadataAddress, options.MetadataAddress);
        Assert.False(options.RequireHttpsMetadata);
        Assert.Null(options.Authority);
    }

    [Fact]
    public void Configure_DefaultScheme_KeepsOriginalClaimNames()
    {
        Assert.False(ConfigureDefaultScheme().MapInboundClaims);
    }

    [Fact]
    public void Configure_DefaultScheme_ValidatesIssuerAudienceLifetimeAndSignature()
    {
        var parameters = ConfigureDefaultScheme().TokenValidationParameters;

        Assert.True(parameters.ValidateIssuer);
        Assert.Equal(Keycloak.ValidIssuer, parameters.ValidIssuer);
        Assert.True(parameters.ValidateAudience);
        Assert.Equal(Keycloak.Audience, parameters.ValidAudience);
        Assert.True(parameters.ValidateLifetime);
        Assert.True(parameters.RequireExpirationTime);
        Assert.True(parameters.ValidateIssuerSigningKey);
        Assert.True(parameters.RequireSignedTokens);
        Assert.Equal([SecurityAlgorithms.RsaSha256], parameters.ValidAlgorithms);
        Assert.Equal(CurrentUser.UsernameClaim, parameters.NameClaimType);
    }

    [Fact]
    public void Configure_OtherScheme_LeavesOptionsUntouched()
    {
        var options = new JwtBearerOptions();

        new ConfigureJwtBearerOptions(Options.Create(Keycloak)).Configure("Other", options);

        Assert.Null(options.MetadataAddress);
        Assert.True(options.MapInboundClaims);
    }

    [Fact]
    public async Task TokenValidated_SubPresent_DoesNotFail()
    {
        var context = await RaiseTokenValidatedAsync(new Claim("sub", "alice-sub"));

        Assert.Null(context.Result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task TokenValidated_SubMissingOrBlank_FailsAuthentication(string? subject)
    {
        var claims = subject is null
            ? new[] { new Claim("preferred_username", "alice") }
            : [new Claim("sub", subject), new Claim("preferred_username", "alice")];

        var context = await RaiseTokenValidatedAsync(claims);

        Assert.NotNull(context.Result);
        Assert.False(context.Result.Succeeded);
        Assert.Equal(ConfigureJwtBearerOptions.MissingSubjectMessage, context.Result.Failure?.Message);
    }

    [Fact]
    public async Task TokenValidated_OnlyMappedNameIdentifier_FailsAuthentication()
    {
        var context = await RaiseTokenValidatedAsync(new Claim(ClaimTypes.NameIdentifier, "alice-sub"));

        Assert.False(context.Result?.Succeeded);
    }
}
