using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.IntegrationTests;

public sealed class PhoneNumbersVisibilityTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task List_BobAfterAliceCreatedPersonalAndShared_SeesOnlyTheSharedEntry()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        using var bob = Fixture.CreateClient(Bob, "bob");
        await CreateAsync(alice, "Alice private", "+420601000001", Visibility.Personal);
        var shared = await CreateAsync(alice, "Alice shared", "+420601000002", Visibility.Shared);

        var entries = await ListAsync(bob);

        var entry = Assert.Single(entries);
        Assert.Equal(shared.Id, entry.Id);
        Assert.False(entry.IsOwnedByCurrentUser);
        Assert.Equal("alice", entry.OwnerUsername);
    }

    [Fact]
    public async Task List_EachScope_ReturnsTheCorrectSubsetNewestFirst()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        using var bob = Fixture.CreateClient(Bob, "bob");
        var alicePersonal = await CreateAsync(alice, "Alice private", "+420601000001", Visibility.Personal);
        Tick();
        var aliceShared = await CreateAsync(alice, "Alice shared", "+420601000002", Visibility.Shared);
        Tick();
        var bobShared = await CreateAsync(bob, "Bob shared", "+420601000003", Visibility.Shared);
        Tick();
        await CreateAsync(bob, "Bob private", "+420601000004", Visibility.Personal);

        var all = await ListAsync(alice);
        var allExplicit = await ListAsync(alice, "?scope=all");
        var personal = await ListAsync(alice, "?scope=personal");
        var shared = await ListAsync(alice, "?scope=SHARED");

        Assert.Equal([bobShared.Id, aliceShared.Id, alicePersonal.Id], all.Select(entry => entry.Id));
        Assert.Equal(all.Select(entry => entry.Id), allExplicit.Select(entry => entry.Id));
        Assert.Equal([alicePersonal.Id], personal.Select(entry => entry.Id));
        Assert.Equal([bobShared.Id, aliceShared.Id], shared.Select(entry => entry.Id));
        Assert.Equal([false, true, true], all.Select(entry => entry.IsOwnedByCurrentUser));
    }

    [Fact]
    public async Task List_EntriesWithTheSameCreationTime_AreOrderedByIdDescending()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        await CreateAsync(alice, "First", "+420601000001", Visibility.Shared);
        await CreateAsync(alice, "Second", "+420601000002", Visibility.Shared);
        await CreateAsync(alice, "Third", "+420601000003", Visibility.Shared);

        var ids = (await ListAsync(alice)).Select(entry => entry.Id).ToList();

        Assert.Equal(ids.OrderByDescending(id => id), ids);
    }

    [Fact]
    public async Task List_ScopePersonalAsBob_DoesNotLeakAlicePersonalEntries()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        using var bob = Fixture.CreateClient(Bob, "bob");
        await CreateAsync(alice, "Alice private", "+420601000001", Visibility.Personal);

        Assert.Empty(await ListAsync(bob, "?scope=personal"));
        Assert.Empty(await ListAsync(bob, "?scope=all"));
        Assert.Empty(await ListAsync(bob, "?scope=shared"));
    }

    [Fact]
    public async Task Get_AlicePersonalEntryAsBob_ReturnsTheSameNotFoundAsAnUnknownId()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        using var bob = Fixture.CreateClient(Bob, "bob");
        var personal = await CreateAsync(alice, "Alice private", "+420601000001", Visibility.Personal);

        var hidden = await bob.GetAsync($"{PhoneNumbersUrl}/{personal.Id}", Cancellation);
        var unknown = await bob.GetAsync($"{PhoneNumbersUrl}/{Guid.CreateVersion7()}", Cancellation);

        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("application/problem+json", hidden.Content.Headers.ContentType?.MediaType);
        var hiddenProblem = await hidden.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        var unknownProblem = await unknown.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        Assert.Equal(unknownProblem.GetProperty("title").GetString(), hiddenProblem.GetProperty("title").GetString());
        Assert.Equal(unknownProblem.GetProperty("type").GetString(), hiddenProblem.GetProperty("type").GetString());
        Assert.Equal(unknownProblem.EnumerateObject().Count(), hiddenProblem.EnumerateObject().Count());
    }

    [Fact]
    public async Task Get_AlicePersonalEntryAsAlice_ReturnsTheEntry()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        var personal = await CreateAsync(alice, "Alice private", "+420601000001", Visibility.Personal);

        var response = await alice.GetAsync($"{PhoneNumbersUrl}/{personal.Id}", Cancellation);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entry = await response.Content.ReadFromJsonAsync<PhoneBook.Api.Application.PhoneNumberResponse>(Json, Cancellation);
        Assert.Equal(personal, entry);
    }

    [Fact]
    public async Task Get_AliceSharedEntryAsBob_ReturnsTheEntry()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        using var bob = Fixture.CreateClient(Bob, "bob");
        var shared = await CreateAsync(alice, "Alice shared", "+420601000002", Visibility.Shared);

        var response = await bob.GetAsync($"{PhoneNumbersUrl}/{shared.Id}", Cancellation);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entry = await response.Content.ReadFromJsonAsync<PhoneBook.Api.Application.PhoneNumberResponse>(Json, Cancellation);
        Assert.False(entry!.IsOwnedByCurrentUser);
    }

    [Fact]
    public async Task List_Response_NeverContainsOwnerId()
    {
        using var alice = Fixture.CreateClient(Alice, "alice");
        await CreateAsync(alice, "Alice shared", "+420601000002", Visibility.Shared);

        var body = await alice.GetStringAsync(PhoneNumbersUrl, Cancellation);

        Assert.DoesNotContain("ownerId", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Alice, body, StringComparison.Ordinal);
    }
}
