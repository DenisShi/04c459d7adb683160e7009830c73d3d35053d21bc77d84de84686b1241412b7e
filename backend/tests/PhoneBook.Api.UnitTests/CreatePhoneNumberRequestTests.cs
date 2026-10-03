using System.ComponentModel.DataAnnotations;
using PhoneBook.Api.Application;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.UnitTests;

public class CreatePhoneNumberRequestTests
{
    private static List<ValidationResult> Validate(CreatePhoneNumberRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }

    private static CreatePhoneNumberRequest Valid() => new()
    {
        ContactName = "Alice Anderson",
        Number = "+420 601 234 567",
        Visibility = Visibility.Personal
    };

    [Fact]
    public void Validate_CompleteRequest_HasNoErrors()
    {
        Assert.Empty(Validate(Valid()));
    }

    [Fact]
    public void Validate_MissingVisibility_ReportsVisibility()
    {
        var request = new CreatePhoneNumberRequest { ContactName = "Alice", Number = "+420601234567" };

        var errors = Validate(request);

        var error = Assert.Single(errors);
        Assert.Contains(nameof(CreatePhoneNumberRequest.Visibility), error.MemberNames);
    }

    [Fact]
    public void Validate_EmptyRequest_ReportsEveryField()
    {
        var members = Validate(new CreatePhoneNumberRequest()).SelectMany(error => error.MemberNames).ToHashSet();

        Assert.Contains(nameof(CreatePhoneNumberRequest.ContactName), members);
        Assert.Contains(nameof(CreatePhoneNumberRequest.Number), members);
        Assert.Contains(nameof(CreatePhoneNumberRequest.Visibility), members);
    }

    [Theory]
    [MemberData(nameof(ValidationContract.InvalidContactNames), MemberType = typeof(ValidationContract))]
    public void Validate_InvalidContactName_ReportsContactName(string contactName)
    {
        var request = new CreatePhoneNumberRequest
        {
            ContactName = contactName,
            Number = "+420601234567",
            Visibility = Visibility.Shared
        };

        var error = Assert.Single(Validate(request));
        Assert.Contains(nameof(CreatePhoneNumberRequest.ContactName), error.MemberNames);
    }

    [Theory]
    [MemberData(nameof(ValidationContract.ValidContactNames), MemberType = typeof(ValidationContract))]
    public void Validate_ValidContactName_HasNoErrors(string contactName, string expectedNormalized)
    {
        var request = new CreatePhoneNumberRequest
        {
            ContactName = contactName,
            Number = "+420601234567",
            Visibility = Visibility.Shared
        };

        Assert.Empty(Validate(request));
        Assert.NotEmpty(expectedNormalized);
    }

    [Theory]
    [MemberData(nameof(ValidationContract.InvalidNumbers), MemberType = typeof(ValidationContract))]
    public void Validate_InvalidNumber_ReportsNumber(string number)
    {
        var request = new CreatePhoneNumberRequest
        {
            ContactName = "Alice",
            Number = number,
            Visibility = Visibility.Shared
        };

        var error = Assert.Single(Validate(request));
        Assert.Contains(nameof(CreatePhoneNumberRequest.Number), error.MemberNames);
    }

    [Theory]
    [MemberData(nameof(ValidationContract.ValidNumbers), MemberType = typeof(ValidationContract))]
    public void Validate_ValidNumber_HasNoErrors(string number, string expectedNormalized)
    {
        var request = new CreatePhoneNumberRequest
        {
            ContactName = "Alice",
            Number = number,
            Visibility = Visibility.Shared
        };

        Assert.Empty(Validate(request));
        Assert.NotEmpty(expectedNormalized);
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Validate_ContactNameLength_IsEnforced(int length, bool valid)
    {
        var request = new CreatePhoneNumberRequest
        {
            ContactName = new string('a', length),
            Number = "+420601234567",
            Visibility = Visibility.Personal
        };

        Assert.Equal(valid, Validate(request).Count == 0);
    }

    [Fact]
    public void Request_HasNoOwnerProperties()
    {
        var names = typeof(CreatePhoneNumberRequest).GetProperties().Select(property => property.Name).ToList();

        Assert.DoesNotContain(names, name => name.Contains("Owner", StringComparison.OrdinalIgnoreCase));
    }
}
