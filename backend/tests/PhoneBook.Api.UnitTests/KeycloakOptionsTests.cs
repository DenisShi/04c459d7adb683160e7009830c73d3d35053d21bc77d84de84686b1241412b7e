using System.ComponentModel.DataAnnotations;
using PhoneBook.Api.Infrastructure.Auth;

namespace PhoneBook.Api.UnitTests;

public class KeycloakOptionsTests
{
    private static KeycloakOptions ValidOptions() => new()
    {
        MetadataAddress = "http://keycloak:8080/realms/phonebook/.well-known/openid-configuration",
        ValidIssuer = "http://localhost:8080/realms/phonebook",
        Audience = "phonebook-api"
    };

    private static List<string> Validate(KeycloakOptions options)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
        return results.SelectMany(result => result.MemberNames).ToList();
    }

    [Fact]
    public void Validate_AllSettingsPresent_HasNoErrors()
    {
        Assert.Empty(Validate(ValidOptions()));
    }

    [Fact]
    public void RequireHttpsMetadata_NotConfigured_DefaultsToTrue()
    {
        Assert.True(new KeycloakOptions().RequireHttpsMetadata);
    }

    [Fact]
    public void Validate_NothingConfigured_ReportsEveryRequiredSetting()
    {
        var errors = Validate(new KeycloakOptions());

        Assert.Contains(nameof(KeycloakOptions.MetadataAddress), errors);
        Assert.Contains(nameof(KeycloakOptions.ValidIssuer), errors);
        Assert.Contains(nameof(KeycloakOptions.Audience), errors);
    }

    [Theory]
    [InlineData("/realms/phonebook")]
    [InlineData("keycloak:8080/realms/phonebook")]
    public void Validate_RelativeUrls_AreRejected(string url)
    {
        var options = ValidOptions();
        options.MetadataAddress = url;
        options.ValidIssuer = url;

        var errors = Validate(options);

        Assert.Contains(nameof(KeycloakOptions.MetadataAddress), errors);
        Assert.Contains(nameof(KeycloakOptions.ValidIssuer), errors);
    }
}
