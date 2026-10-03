namespace PhoneBook.Api.Application;

public enum PhoneNumberScope
{
    All,
    Personal,
    Shared
}

public static class PhoneNumberScopes
{
    public static bool TryParse(string? value, out PhoneNumberScope scope)
    {
        scope = PhoneNumberScope.All;
        if (value is null)
        {
            return true;
        }

        foreach (var candidate in Enum.GetValues<PhoneNumberScope>())
        {
            if (string.Equals(value, candidate.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                scope = candidate;
                return true;
            }
        }

        return false;
    }
}
