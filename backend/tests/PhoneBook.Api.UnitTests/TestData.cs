using PhoneBook.Api.Domain;
using Microsoft.Extensions.Time.Testing;

namespace PhoneBook.Api.UnitTests;

public static class TestData
{
    public const string Alice = "alice-sub";
    public const string Bob = "bob-sub";

    public static readonly DateTimeOffset Now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    public static PhoneNumber Entry(string ownerId, Visibility visibility, string ownerUsername = "owner")
    {
        var clock = new FakeTimeProvider(Now);
        return PhoneNumber.Create("Contact", "+420601234567", visibility, ownerId, ownerUsername, clock);
    }
}
