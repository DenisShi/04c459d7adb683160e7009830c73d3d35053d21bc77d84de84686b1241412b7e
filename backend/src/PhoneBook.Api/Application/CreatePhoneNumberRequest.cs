using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.Application;

public sealed class CreatePhoneNumberRequest
{
    [Required]
    [ContactName]
    [Description("Name of the contact. Surrounding whitespace is trimmed; 1 to 100 characters.")]
    public string? ContactName { get; init; }

    [Required]
    [PhoneNumberValue]
    [Description("Phone number in international format: a leading plus, the country calling code and the national number, for example +420 601 234 567. Spaces, dashes, dots and parentheses are allowed. The number must be valid for its country and is stored in E.164 form.")]
    public string? Number { get; init; }

    [Required]
    [Description("PERSONAL is visible only to the creator, SHARED is visible to every signed-in user.")]
    public Visibility? Visibility { get; init; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ContactNameAttribute : ValidationAttribute
{
    public ContactNameAttribute()
        : base($"The contact name must be 1 to {PhoneNumberFormat.MaxContactNameLength} characters long.")
    {
    }

    public override bool IsValid(object? value) =>
        value is null || (value is string text && PhoneNumberFormat.TryNormalizeContactName(text, out _));
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class PhoneNumberValueAttribute : ValidationAttribute
{
    public PhoneNumberValueAttribute()
        : base("Enter a valid phone number in international format, for example +420 601 234 567.")
    {
    }

    public override bool IsValid(object? value) =>
        value is null || (value is string text && PhoneNumberFormat.TryNormalizeNumber(text, out _));
}
