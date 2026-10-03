using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PhoneBook.Api.Application;
using PhoneBook.Api.Domain;
using PhoneBook.Api.Infrastructure.Persistence;

namespace PhoneBook.Api.IntegrationTests;

public sealed class PhoneNumbersValidationTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    public static TheoryData<string, string[]> InvalidBodies => new()
    {
        { """{"contactName":"","number":"+420601234567","visibility":"PERSONAL"}""", ["contactName"] },
        { """{"contactName":"   ","number":"+420601234567","visibility":"PERSONAL"}""", ["contactName"] },
        { """{"number":"+420601234567","visibility":"PERSONAL"}""", ["contactName"] },
        { """{"contactName":"Alice","number":"12","visibility":"PERSONAL"}""", ["number"] },
        { """{"contactName":"Alice","number":"420+601","visibility":"SHARED"}""", ["number"] },
        { """{"contactName":"Alice","visibility":"SHARED"}""", ["number"] },
        { """{"contactName":"Alice","number":"+420601234567"}""", ["visibility"] },
        { """{"contactName":"","number":"abc"}""", ["contactName", "number", "visibility"] }
    };

    [Theory]
    [MemberData(nameof(InvalidBodies))]
    public async Task Post_InvalidBody_ReturnsValidationProblemWithCamelCaseKeys(string body, string[] expectedKeys)
    {
        using var alice = Fixture.CreateClient(Alice, "alice");

        var response = await alice.PostAsync(PhoneNumbersUrl, JsonBody(body), Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        var keys = problem.GetProperty("errors").EnumerateObject().Select(property => property.Name).ToList();
        foreach (var expected in expectedKeys)
        {
            Assert.Contains(expected, keys);
        }

        Assert.Empty(await ListAsync(alice));
    }

    [Theory]
    [InlineData("""{"contactName":"Alice","number":"+420601234567","visibility":"FOO"}""")]
    [InlineData("""{"contactName":"Alice","number":"+420601234567","visibility":1}""")]
    [InlineData("""{"contactName":"Alice","number":"+420601234567","visibility":0}""")]
    [InlineData("""{"contactName":"Alice","number":"+420601234567","visibility":null}""")]
    [InlineData("""{"contactName":"Alice","number":""")]
    [InlineData("not json")]
    public async Task Post_UnknownOrMalformedVisibilityOrJson_ReturnsBadRequestProblem(string body)
    {
        using var alice = Fixture.CreateClient(Alice, "alice");

        var response = await alice.PostAsync(PhoneNumbersUrl, JsonBody(body), Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Empty(await ListAsync(alice));
    }

    [Theory]
    [InlineData(100, HttpStatusCode.Created)]
    [InlineData(101, HttpStatusCode.BadRequest)]
    public async Task Post_ContactNameLength_IsEnforced(int length, HttpStatusCode expected)
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        var body = $$"""{"contactName":"{{new string('a', length)}}","number":"+420601234567","visibility":"SHARED"}""";

        var response = await alice.PostAsync(PhoneNumbersUrl, JsonBody(body), Cancellation);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Post_ValidBody_ReturnsCreatedWithNormalizedNumberAndResolvableLocation()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        var body = """{"contactName":"  Alice Anderson ","number":"+420 601 234 567","visibility":"PERSONAL"}""";

        var response = await alice.PostAsync(PhoneNumbersUrl, JsonBody(body), Cancellation);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<PhoneNumberResponse>(Json, Cancellation);
        Assert.Equal("Alice Anderson", created!.ContactName);
        Assert.Equal("+420601234567", created.Number);
        Assert.Equal(Visibility.Personal, created.Visibility);
        Assert.True(created.IsOwnedByCurrentUser);
        Assert.Equal(Fixture.Clock.GetUtcNow(), created.CreatedAt);
        Assert.Equal($"{PhoneNumbersUrl}/{created.Id}", response.Headers.Location?.OriginalString);
        var fetched = await alice.GetFromJsonAsync<PhoneNumberResponse>(response.Headers.Location, Json, Cancellation);
        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Post_ResponseVisibility_IsSerializedAsUpperCaseString()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        var body = """{"contactName":"Alice","number":"+420601234567","visibility":"SHARED"}""";

        var response = await alice.PostAsync(PhoneNumbersUrl, JsonBody(body), Cancellation);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        Assert.Equal("SHARED", json.GetProperty("visibility").GetString());
        Assert.True(json.TryGetProperty("createdAt", out _));
        Assert.True(json.TryGetProperty("isOwnedByCurrentUser", out _));
    }

    [Fact]
    public async Task Post_BodyWithOwnerFields_IgnoresThemAndUsesTheCaller()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        var body = """
            {"contactName":"Alice","number":"+420601234567","visibility":"PERSONAL",
             "ownerId":"bob-sub","ownerUsername":"bob","OwnerId":"bob-sub"}
            """;

        var response = await alice.PostAsync(PhoneNumbersUrl, JsonBody(body), Cancellation);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<PhoneNumberResponse>(Json, Cancellation);
        Assert.Equal("alice", created!.OwnerUsername);
        Assert.True(created.IsOwnedByCurrentUser);
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await dbContext.PhoneNumbers.SingleAsync(Cancellation);
        Assert.Equal(Alice, stored.OwnerId);
        Assert.Equal("alice", stored.OwnerUsername);
    }

    [Fact]
    public async Task Post_QueryAndHeadersNamingAnotherOwner_AreIgnored()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{PhoneNumbersUrl}?ownerId={Bob}&owner={Bob}")
        {
            Content = JsonBody("""{"contactName":"Alice","number":"+420601234567","visibility":"SHARED"}""")
        };
        request.Headers.Add("X-Owner-Id", Bob);

        var response = await alice.SendAsync(request, Cancellation);

        var created = await response.Content.ReadFromJsonAsync<PhoneNumberResponse>(Json, Cancellation);
        Assert.Equal("alice", created!.OwnerUsername);
    }

    [Fact]
    public async Task Post_NonJsonContentType_ReturnsUnsupportedMediaTypeProblem()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        using var content = new StringContent("contactName=Alice", Encoding.UTF8, "text/plain");

        var response = await alice.PostAsync(PhoneNumbersUrl, content, Cancellation);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("bogus")]
    [InlineData("0")]
    [InlineData("1")]
    public async Task List_UnknownScope_ReturnsBadRequestProblem(string scope)
    {
        using var alice = Fixture.CreateClient(Alice, "alice");

        var response = await alice.GetAsync($"{PhoneNumbersUrl}?scope={scope}", Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        Assert.True(problem.GetProperty("errors").TryGetProperty("scope", out _));
    }

    [Fact]
    public async Task Database_NumberOutsideCanonicalFormat_IsRejectedByCheckConstraint()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var exception = await Assert.ThrowsAsync<PostgresException>(() => dbContext.Database.ExecuteSqlRawAsync(
            "INSERT INTO phone_numbers (id, contact_name, number, visibility, owner_id, owner_username, created_at) " +
            "VALUES (gen_random_uuid(), 'x', '12', 'SHARED', 'o', 'o', now())",
            Cancellation));

        Assert.Equal("ck_phone_numbers_number_format", exception.ConstraintName);
    }
}
