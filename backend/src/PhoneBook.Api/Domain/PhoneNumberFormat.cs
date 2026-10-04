using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using PhoneNumbers;

namespace PhoneBook.Api.Domain;

public static partial class PhoneNumberFormat
{
    public const int MaxContactNameLength = 100;
    public const int MaxRawNumberLength = 32;

    private static readonly PhoneNumberUtil NumberingPlan = PhoneNumberUtil.GetInstance();

    public static bool TryNormalizeContactName(string? input, [NotNullWhen(true)] out string? normalized)
    {
        normalized = null;
        var trimmed = input?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxContactNameLength)
        {
            return false;
        }

        normalized = trimmed;
        return true;
    }

    public static bool TryNormalizeNumber(string? input, [NotNullWhen(true)] out string? normalized)
    {
        normalized = null;
        var trimmed = input?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxRawNumberLength)
        {
            return false;
        }

        var builder = new StringBuilder(trimmed.Length);
        foreach (var character in trimmed)
        {
            if (!IsSeparator(character))
            {
                builder.Append(character);
            }
        }

        var candidate = builder.ToString();
        if (!InternationalNumber().IsMatch(candidate))
        {
            return false;
        }

        PhoneNumbers.PhoneNumber parsed;
        try
        {
            parsed = NumberingPlan.Parse(candidate, null);
        }
        catch (NumberParseException)
        {
            return false;
        }

        if (!NumberingPlan.IsValidNumber(parsed))
        {
            return false;
        }

        normalized = NumberingPlan.Format(parsed, PhoneNumbers.PhoneNumberFormat.E164);
        return true;
    }

    private static bool IsSeparator(char character) => character is ' ' or '-' or '.' or '(' or ')';

    [GeneratedRegex(@"^\+[0-9]{1,17}\z")]
    private static partial Regex InternationalNumber();
}
