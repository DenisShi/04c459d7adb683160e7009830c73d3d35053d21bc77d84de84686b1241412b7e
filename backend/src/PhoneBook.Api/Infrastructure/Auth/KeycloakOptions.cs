using System.ComponentModel.DataAnnotations;

namespace PhoneBook.Api.Infrastructure.Auth;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    [Required]
    [Url]
    public string MetadataAddress { get; set; } = string.Empty;

    [Required]
    [Url]
    public string ValidIssuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    public bool RequireHttpsMetadata { get; set; } = true;
}
