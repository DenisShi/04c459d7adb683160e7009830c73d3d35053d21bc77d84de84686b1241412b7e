using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PhoneBook.Api.Application;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public abstract class ApiTestBase(ApiFixture fixture) : IAsyncLifetime
{
    protected const string Alice = "alice-sub";
    protected const string Bob = "bob-sub";
    protected const string PhoneNumbersUrl = "/api/phone-numbers";

    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };

    protected ApiFixture Fixture { get; } = fixture;

    protected static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await Fixture.ResetAsync();

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    protected static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    protected static async Task<PhoneNumberResponse> CreateAsync(
        HttpClient client,
        string contactName,
        string number,
        Visibility visibility)
    {
        var body = new { contactName, number, visibility = visibility == Visibility.Shared ? "SHARED" : "PERSONAL" };
        var response = await client.PostAsJsonAsync(PhoneNumbersUrl, body, Json, Cancellation);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PhoneNumberResponse>(Json, Cancellation))!;
    }

    protected static async Task<IReadOnlyList<PhoneNumberResponse>> ListAsync(HttpClient client, string query = "")
    {
        var response = await client.GetAsync(PhoneNumbersUrl + query, Cancellation);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<PhoneNumberResponse>>(Json, Cancellation))!;
    }

    protected void Tick() => Fixture.Clock.Advance(TimeSpan.FromSeconds(1));
}
