using System.ComponentModel;
using System.Linq.Expressions;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.Application;

public sealed record PhoneNumberResponse(
    [property: Description("Identifier of the entry.")] Guid Id,
    [property: Description("Name of the contact.")] string ContactName,
    [property: Description("Phone number in canonical form.")] string Number,
    [property: Description("PERSONAL or SHARED.")] Visibility Visibility,
    [property: Description("Username of the user who created the entry.")] string OwnerUsername,
    [property: Description("True when the signed-in user created the entry.")] bool IsOwnedByCurrentUser,
    [property: Description("Creation time in UTC.")] DateTimeOffset CreatedAt)
{
    public static Expression<Func<PhoneNumber, PhoneNumberResponse>> ProjectionFor(string subject) =>
        entry => new PhoneNumberResponse(
            entry.Id,
            entry.ContactName,
            entry.Number,
            entry.Visibility,
            entry.OwnerUsername,
            entry.OwnerId == subject,
            entry.CreatedAt);

    public static PhoneNumberResponse From(PhoneNumber entry, string subject)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return new PhoneNumberResponse(
            entry.Id,
            entry.ContactName,
            entry.Number,
            entry.Visibility,
            entry.OwnerUsername,
            entry.OwnerId == subject,
            entry.CreatedAt);
    }
}
