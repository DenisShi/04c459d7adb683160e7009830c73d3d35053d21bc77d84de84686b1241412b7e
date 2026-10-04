using PhoneBook.Api.Domain;

namespace PhoneBook.Api.UnitTests;

public class PhoneNumberFormatTests
{
    [Theory]
    [MemberData(nameof(ValidationContract.ValidContactNames), MemberType = typeof(ValidationContract))]
    public void TryNormalizeContactName_ValidInput_ReturnsTrimmedName(string input, string expected)
    {
        var result = PhoneNumberFormat.TryNormalizeContactName(input, out var normalized);

        Assert.True(result);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [MemberData(nameof(ValidationContract.InvalidContactNames), MemberType = typeof(ValidationContract))]
    public void TryNormalizeContactName_InvalidInput_ReturnsFalse(string input)
    {
        Assert.False(PhoneNumberFormat.TryNormalizeContactName(input, out var normalized));
        Assert.Null(normalized);
    }

    [Fact]
    public void TryNormalizeContactName_Null_ReturnsFalse()
    {
        Assert.False(PhoneNumberFormat.TryNormalizeContactName(null, out _));
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void TryNormalizeContactName_LengthBoundary_IsEnforced(int length, bool expected)
    {
        var name = new string('a', length);

        Assert.Equal(expected, PhoneNumberFormat.TryNormalizeContactName(name, out _));
    }

    [Fact]
    public void TryNormalizeContactName_WhitespaceAroundMaximumLength_IsTrimmedBeforeCounting()
    {
        var name = "  " + new string('a', 100) + "  ";

        Assert.True(PhoneNumberFormat.TryNormalizeContactName(name, out var normalized));
        Assert.Equal(100, normalized!.Length);
    }

    [Theory]
    [MemberData(nameof(ValidationContract.ValidNumbers), MemberType = typeof(ValidationContract))]
    public void TryNormalizeNumber_ValidInput_ReturnsCanonicalForm(string input, string expected)
    {
        var result = PhoneNumberFormat.TryNormalizeNumber(input, out var normalized);

        Assert.True(result);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [MemberData(nameof(ValidationContract.InvalidNumbers), MemberType = typeof(ValidationContract))]
    public void TryNormalizeNumber_InvalidInput_ReturnsFalse(string input)
    {
        Assert.False(PhoneNumberFormat.TryNormalizeNumber(input, out var normalized));
        Assert.Null(normalized);
    }

    [Fact]
    public void TryNormalizeNumber_Null_ReturnsFalse()
    {
        Assert.False(PhoneNumberFormat.TryNormalizeNumber(null, out _));
    }

    [Theory]
    [InlineData(32, true)]
    [InlineData(33, false)]
    public void TryNormalizeNumber_RawLengthBoundary_IsEnforced(int rawLength, bool expected)
    {
        var number = "+420" + new string(' ', rawLength - 13) + "601234567";

        Assert.Equal(rawLength, number.Length);
        Assert.Equal(expected, PhoneNumberFormat.TryNormalizeNumber(number, out _));
    }

    [Theory]
    [InlineData("+420 60123456", false)]
    [InlineData("+420 601234567", true)]
    [InlineData("+420 6012345678", false)]
    public void TryNormalizeNumber_DigitCount_FollowsTheCountryNumberingPlan(string number, bool expected)
    {
        Assert.Equal(expected, PhoneNumberFormat.TryNormalizeNumber(number, out _));
    }

    [Fact]
    public void TryNormalizeNumber_NationalNumberWithoutCountryCode_ReturnsFalse()
    {
        Assert.False(PhoneNumberFormat.TryNormalizeNumber("601 234 567", out _));
    }

    [Fact]
    public void TryNormalizeNumber_TrunkPrefixAfterCountryCode_IsDroppedFromCanonicalForm()
    {
        Assert.True(PhoneNumberFormat.TryNormalizeNumber("+44 (0) 20 7946 0958", out var normalized));
        Assert.Equal("+442079460958", normalized);
    }
}
