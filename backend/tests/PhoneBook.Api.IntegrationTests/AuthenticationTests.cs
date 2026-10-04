using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PhoneBook.Api.Domain;
using PhoneBook.Api.Infrastructure.Persistence;

namespace PhoneBook.Api.IntegrationTests;

public sealed class AuthenticationTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    private const string AliceSubject = "8a1f6c2e-0b6d-4d0e-9a51-6f1f0c9a0001";
    private const string BobSubject = "8a1f6c2e-0b6d-4d0e-9a51-6f1f0c9a0002";

    public static TheoryData<string> InvalidTokenKinds =>
    [
        "wrong-issuer",
        "wrong-audience",
        "id-token-audience",
        "expired",
        "not-yet-valid",
        "missing-sub",
        "blank-sub",
        "foreign-signing-key",
        "symmetric-signature",
        "unsigned",
        "tampered-payload",
        "tampered-signature",
        "not-a-jwt"
    ];

    public static TheoryData<string, bool> MutatingRequests
    {
        get
        {
            var data = new TheoryData<string, bool>();
            foreach (var method in new[] { "PUT", "PATCH", "DELETE" })
            {
                data.Add(method, false);
                data.Add(method, true);
            }

            return data;
        }
    }

    [Fact]
    public async Task ValidToken_AliceListsAll_SeesHerPersonalAndEveryShared()
    {
        using var alice = AliceClient();
        using var bob = BobClient();
        var alicePersonal = await CreateAsync(alice, "Alice private", "+420601000001", Visibility.Personal);
        var aliceShared = await CreateAsync(alice, "Alice shared", "+420601000002", Visibility.Shared);
        await CreateAsync(bob, "Bob private", "+420601000003", Visibility.Personal);
        var bobShared = await CreateAsync(bob, "Bob shared", "+420601000004", Visibility.Shared);

        var entries = await ListAsync(alice);

        Assert.Equal(
            new[] { alicePersonal.Id, aliceShared.Id, bobShared.Id }.Order(),
            entries.Select(entry => entry.Id).Order());
        Assert.True(entries.Single(entry => entry.Id == alicePersonal.Id).IsOwnedByCurrentUser);
        Assert.False(entries.Single(entry => entry.Id == bobShared.Id).IsOwnedByCurrentUser);
    }

    [Fact]
    public async Task ValidToken_BobListsAndFetches_NeverSeesAlicePersonalEntry()
    {
        using var alice = AliceClient();
        using var bob = BobClient();
        var alicePersonal = await CreateAsync(alice, "Alice private", "+420601000001", Visibility.Personal);
        var aliceShared = await CreateAsync(alice, "Alice shared", "+420601000002", Visibility.Shared);

        foreach (var scope in new[] { "", "?scope=all", "?scope=personal", "?scope=shared" })
        {
            Assert.DoesNotContain(alicePersonal.Id, (await ListAsync(bob, scope)).Select(entry => entry.Id));
        }

        Assert.Equal([aliceShared.Id], (await ListAsync(bob)).Select(entry => entry.Id));
        var direct = await bob.GetAsync($"{PhoneNumbersUrl}/{alicePersonal.Id}", Cancellation);
        Assert.Equal(HttpStatusCode.NotFound, direct.StatusCode);
    }

    [Fact]
    public async Task ValidToken_Post_StoresSubAsOwnerAndPreferredUsernameAsUsername()
    {
        using var alice = AliceClient();

        var created = await CreateAsync(alice, "Alice", "+420601000001", Visibility.Personal);

        Assert.Equal("alice", created.OwnerUsername);
        Assert.True(created.IsOwnedByCurrentUser);
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await dbContext.PhoneNumbers.AsNoTracking().SingleAsync(Cancellation);
        Assert.Equal(AliceSubject, stored.OwnerId);
        Assert.Equal("alice", stored.OwnerUsername);
    }

    [Fact]
    public async Task ValidTokenWithoutPreferredUsername_Post_FallsBackToSubAsUsername()
    {
        using var client = Fixture.CreateJwtClient(Fixture.Tokens.Create(AliceSubject, username: null));

        var created = await CreateAsync(client, "Alice", "+420601000001", Visibility.Shared);

        Assert.Equal(AliceSubject, created.OwnerUsername);
    }

    [Theory]
    [InlineData("GET", PhoneNumbersUrl)]
    [InlineData("GET", PhoneNumbersUrl + "/0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5b")]
    [InlineData("POST", PhoneNumbersUrl)]
    public async Task NoToken_ReturnsUnauthorizedProblemWithBearerChallenge(string method, string url)
    {
        using var anonymous = Fixture.CreateJwtClient(token: null);
        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "POST")
        {
            request.Content = JsonBody("""{"contactName":"Alice","number":"+420601234567","visibility":"SHARED"}""");
        }

        var response = await anonymous.SendAsync(request, Cancellation);

        await AssertUnauthorizedProblemAsync(response);
        Assert.Contains(response.Headers.WwwAuthenticate, header => header.Scheme == "Bearer");
    }

    [Fact]
    public async Task NoTestCredentials_ReturnsUnauthorizedProblem()
    {
        using var anonymous = Fixture.Factory.CreateClient();

        var response = await anonymous.GetAsync(PhoneNumbersUrl, Cancellation);

        await AssertUnauthorizedProblemAsync(response);
    }

    [Theory]
    [MemberData(nameof(InvalidTokenKinds))]
    public async Task InvalidToken_ReturnsUnauthorizedAndStoresNothing(string kind)
    {
        using var client = Fixture.CreateJwtClient(CreateInvalidToken(kind));

        var list = await client.GetAsync(PhoneNumbersUrl, Cancellation);
        var create = await client.PostAsync(
            PhoneNumbersUrl,
            JsonBody("""{"contactName":"Alice","number":"+420601234567","visibility":"SHARED"}"""),
            Cancellation);

        await AssertUnauthorizedProblemAsync(list);
        await AssertUnauthorizedProblemAsync(create);
        using var alice = AliceClient();
        Assert.Empty(await ListAsync(alice));
    }

    [Theory]
    [MemberData(nameof(MutatingRequests))]
    public async Task ValidToken_MutatingMethod_ReturnsMethodNotAllowedAndKeepsTheEntry(string method, bool itemRoute)
    {
        using var alice = AliceClient();
        var entry = await CreateAsync(alice, "Alice", "+420601234567", Visibility.Shared);
        var url = itemRoute ? $"{PhoneNumbersUrl}/{entry.Id}" : PhoneNumbersUrl;
        using var request = new HttpRequestMessage(new HttpMethod(method), url)
        {
            Content = JsonBody("""{"contactName":"Changed","number":"+420601234567","visibility":"SHARED"}""")
        };

        var response = await alice.SendAsync(request, Cancellation);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.Contains("GET", response.Content.Headers.Allow);
        Assert.Equal("Alice", Assert.Single(await ListAsync(alice)).ContactName);
    }

    [Theory]
    [MemberData(nameof(MutatingRequests))]
    public async Task NoToken_MutatingMethod_ReturnsUnauthorizedBeforeMethodNotAllowed(string method, bool itemRoute)
    {
        using var alice = AliceClient();
        var entry = await CreateAsync(alice, "Alice", "+420601234567", Visibility.Shared);
        using var anonymous = Fixture.CreateJwtClient(token: null);
        var url = itemRoute ? $"{PhoneNumbersUrl}/{entry.Id}" : PhoneNumbersUrl;
        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        var response = await anonymous.SendAsync(request, Cancellation);

        await AssertUnauthorizedProblemAsync(response);
        Assert.Single(await ListAsync(alice));
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    [InlineData("/api/openapi/v1.json")]
    public async Task AnonymousEndpoints_WithRealJwtBearer_ReturnOk(string path)
    {
        using var anonymous = Fixture.CreateJwtClient(token: null);

        var response = await anonymous.GetAsync(path, Cancellation);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient AliceClient() => Fixture.CreateJwtClient(Fixture.Tokens.Create(AliceSubject, "alice"));

    private HttpClient BobClient() => Fixture.CreateJwtClient(Fixture.Tokens.Create(BobSubject, "bob"));

    private string CreateInvalidToken(string kind)
    {
        var tokens = Fixture.Tokens;
        return kind switch
        {
            "wrong-issuer" => tokens.Create(AliceSubject, "alice", token => token.Issuer = "http://keycloak:8080/realms/phonebook"),
            "wrong-audience" => tokens.Create(AliceSubject, "alice", token => token.Audience = "another-api"),
            "id-token-audience" => tokens.Create(AliceSubject, "alice", token =>
            {
                token.Audience = "phonebook-spa";
                token.Claims["typ"] = "ID";
            }),
            "expired" => tokens.Create(AliceSubject, "alice", token =>
            {
                token.IssuedAt = DateTime.UtcNow.AddMinutes(-30);
                token.NotBefore = DateTime.UtcNow.AddMinutes(-30);
                token.Expires = DateTime.UtcNow.AddMinutes(-10);
            }),
            "not-yet-valid" => tokens.Create(AliceSubject, "alice", token =>
            {
                token.IssuedAt = DateTime.UtcNow.AddMinutes(10);
                token.NotBefore = DateTime.UtcNow.AddMinutes(10);
                token.Expires = DateTime.UtcNow.AddMinutes(15);
            }),
            "missing-sub" => tokens.Create(subject: null, "alice"),
            "blank-sub" => tokens.Create(" ", "alice"),
            "foreign-signing-key" => tokens.Create(AliceSubject, "alice", token =>
                token.SigningCredentials = new SigningCredentials(
                    new RsaSecurityKey(RSA.Create(2048)) { KeyId = TestTokens.KeyId },
                    SecurityAlgorithms.RsaSha256)),
            "symmetric-signature" => tokens.Create(AliceSubject, "alice", token =>
                token.SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)) { KeyId = TestTokens.KeyId },
                    SecurityAlgorithms.HmacSha256)),
            "unsigned" => tokens.Create(AliceSubject, "alice", token => token.SigningCredentials = null),
            "tampered-payload" => TamperPayload(tokens.Create(AliceSubject, "alice")),
            "tampered-signature" => TamperSignature(tokens.Create(AliceSubject, "alice")),
            "not-a-jwt" => "not-a-jwt",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown invalid token kind.")
        };
    }

    private static string TamperPayload(string token)
    {
        var parts = token.Split('.');
        var payload = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(parts[1]))
            .Replace(AliceSubject, BobSubject, StringComparison.Ordinal);
        parts[1] = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payload));
        return string.Join('.', parts);
    }

    private static string TamperSignature(string token)
    {
        var parts = token.Split('.');
        var signature = Base64UrlEncoder.DecodeBytes(parts[2]);
        signature[0] ^= 0xFF;
        parts[2] = Base64UrlEncoder.Encode(signature);
        return string.Join('.', parts);
    }

    private static async Task AssertUnauthorizedProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(Cancellation);
        Assert.Equal(401, problem.GetProperty("status").GetInt32());
    }
}
