using System.Text.Json;

namespace PhoneBook.Api.UnitTests;

public static class ValidationContract
{
    private static readonly JsonElement Root = Load();

    public static TheoryData<string, string> ValidContactNames => Valid("contactName");

    public static TheoryData<string> InvalidContactNames => Invalid("contactName");

    public static TheoryData<string, string> ValidNumbers => Valid("number");

    public static TheoryData<string> InvalidNumbers => Invalid("number");

    private static TheoryData<string, string> Valid(string field)
    {
        var data = new TheoryData<string, string>();
        foreach (var item in Root.GetProperty(field).GetProperty("valid").EnumerateArray())
        {
            data.Add(item.GetProperty("input").GetString()!, item.GetProperty("normalized").GetString()!);
        }

        return data;
    }

    private static TheoryData<string> Invalid(string field)
    {
        var data = new TheoryData<string>();
        foreach (var item in Root.GetProperty(field).GetProperty("invalid").EnumerateArray())
        {
            data.Add(item.GetString()!);
        }

        return data;
    }

    private static JsonElement Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "contracts", "phone-number-validation-cases.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }
}
