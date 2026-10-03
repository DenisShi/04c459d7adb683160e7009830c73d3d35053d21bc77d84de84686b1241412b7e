namespace PhoneBook.Api.Domain;

public sealed class PhoneNumber
{
    private PhoneNumber()
    {
    }

    public Guid Id { get; private set; }

    public string ContactName { get; private set; } = string.Empty;

    public string Number { get; private set; } = string.Empty;

    public Visibility Visibility { get; private set; }

    public string OwnerId { get; private set; } = string.Empty;

    public string OwnerUsername { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static PhoneNumber Create(
        string contactName,
        string number,
        Visibility visibility,
        string ownerId,
        string ownerUsername,
        TimeProvider timeProvider)
    {
        if (!PhoneNumberFormat.TryNormalizeContactName(contactName, out var normalizedName))
        {
            throw new ArgumentException("The contact name is not valid.", nameof(contactName));
        }

        if (!PhoneNumberFormat.TryNormalizeNumber(number, out var normalizedNumber))
        {
            throw new ArgumentException("The phone number is not valid.", nameof(number));
        }

        if (!Enum.IsDefined(visibility))
        {
            throw new ArgumentOutOfRangeException(nameof(visibility));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUsername);

        var createdAt = timeProvider.GetUtcNow();
        return new PhoneNumber
        {
            Id = Guid.CreateVersion7(createdAt),
            ContactName = normalizedName,
            Number = normalizedNumber,
            Visibility = visibility,
            OwnerId = ownerId,
            OwnerUsername = ownerUsername,
            CreatedAt = createdAt
        };
    }
}
