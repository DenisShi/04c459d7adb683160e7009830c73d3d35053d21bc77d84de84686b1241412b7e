using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhoneBook.Api.Domain;
using PhoneBook.Api.Infrastructure.Persistence;

namespace PhoneBook.Api.IntegrationTests;

public sealed class ReadOnlyApiSurfaceTests(ApiFixture fixture) : ApiTestBase(fixture)
{
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

    [Theory]
    [MemberData(nameof(MutatingRequests))]
    public async Task MutatingMethods_OnCollectionAndItemRoutes_ReturnMethodNotAllowedWithAllowHeader(
        string method,
        bool itemRoute)
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        var entry = await CreateAsync(alice, "Alice", "+420601234567", Visibility.Shared);
        var url = itemRoute ? $"{PhoneNumbersUrl}/{entry.Id}" : PhoneNumbersUrl;
        using var request = new HttpRequestMessage(new HttpMethod(method), url)
        {
            Content = JsonBody("""{"contactName":"Changed","number":"+420601234567","visibility":"SHARED"}""")
        };

        var response = await alice.SendAsync(request, Cancellation);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.Contains("GET", response.Content.Headers.Allow);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var stored = Assert.Single(await ListAsync(alice));
        Assert.Equal("Alice", stored.ContactName);
    }

    [Fact]
    public async Task AppDbContext_ModifyingTrackedEntry_Throws()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entry = PhoneNumber.Create("Alice", "+420601234567", Visibility.Shared, Alice, "alice", Fixture.Clock);
        dbContext.PhoneNumbers.Add(entry);
        await dbContext.SaveChangesAsync(Cancellation);

        dbContext.Entry(entry).Property(nameof(PhoneNumber.ContactName)).CurrentValue = "Changed";

        await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.SaveChangesAsync(Cancellation));
    }

    [Fact]
    public async Task AppDbContext_RemovingTrackedEntry_Throws()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entry = PhoneNumber.Create("Alice", "+420601234567", Visibility.Shared, Alice, "alice", Fixture.Clock);
        dbContext.PhoneNumbers.Add(entry);
        await dbContext.SaveChangesAsync(Cancellation);

        dbContext.PhoneNumbers.Remove(entry);

        await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.SaveChangesAsync(Cancellation));
        Assert.Equal(1, await dbContext.PhoneNumbers.AsNoTracking().CountAsync(Cancellation));
    }

    [Fact]
    public async Task AppDbContext_SynchronousSaveOfModifiedEntry_Throws()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entry = PhoneNumber.Create("Alice", "+420601234567", Visibility.Shared, Alice, "alice", Fixture.Clock);
        dbContext.PhoneNumbers.Add(entry);
        dbContext.SaveChanges();

        dbContext.Entry(entry).Property(nameof(PhoneNumber.ContactName)).CurrentValue = "Changed";

        Assert.Throws<InvalidOperationException>(() => dbContext.SaveChanges());
    }

    [Fact]
    public async Task AppDbContext_SynchronousSaveOfRemovedEntry_Throws()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entry = PhoneNumber.Create("Alice", "+420601234567", Visibility.Shared, Alice, "alice", Fixture.Clock);
        dbContext.PhoneNumbers.Add(entry);
        dbContext.SaveChanges();

        dbContext.PhoneNumbers.Remove(entry);

        Assert.Throws<InvalidOperationException>(() => dbContext.SaveChanges());
        Assert.Equal(1, await dbContext.PhoneNumbers.AsNoTracking().CountAsync(Cancellation));
    }

    [Fact]
    public async Task AppDbContext_AddingEntries_Succeeds()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.PhoneNumbers.Add(PhoneNumber.Create("A", "+420601234567", Visibility.Shared, Alice, "alice", Fixture.Clock));
        dbContext.PhoneNumbers.Add(PhoneNumber.Create("B", "+420601234568", Visibility.Personal, Bob, "bob", Fixture.Clock));

        Assert.Equal(2, await dbContext.SaveChangesAsync(Cancellation));
    }

    [Fact]
    public async Task OpenApiDocument_IsAnonymousAndDeclaresNoMutatingOperations()
    {
        using var anonymous = Fixture.Factory.CreateClient();

        var response = await anonymous.GetAsync("/api/openapi/v1.json", Cancellation);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        var paths = document.RootElement.GetProperty("paths");
        var methods = paths.EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject().Select(operation => operation.Name))
            .ToHashSet();
        Assert.Contains("get", methods);
        Assert.Contains("post", methods);
        Assert.DoesNotContain("put", methods);
        Assert.DoesNotContain("patch", methods);
        Assert.DoesNotContain("delete", methods);
        Assert.True(document.RootElement.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoints_WithoutCredentials_ReturnOk(string path)
    {
        using var anonymous = Fixture.Factory.CreateClient();

        var response = await anonymous.GetAsync(path, Cancellation);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Startup_AppliesInitialMigration()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var applied = await dbContext.Database.GetAppliedMigrationsAsync(Cancellation);
        var pending = await dbContext.Database.GetPendingMigrationsAsync(Cancellation);

        Assert.Contains(applied, migration => migration.EndsWith("_Initial", StringComparison.Ordinal));
        Assert.Empty(pending);
    }
}
