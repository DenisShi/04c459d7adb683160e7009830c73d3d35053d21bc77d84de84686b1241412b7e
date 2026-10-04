using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using PhoneBook.Api.Infrastructure.Persistence;

namespace PhoneBook.Api.UnitTests;

public class PersistenceRegistrationTests
{
    private static ServiceProvider BuildProvider(params KeyValuePair<string, string?>[] settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPersistence(configuration);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void ResolveDbContext_WithoutConnectionString_ThrowsNamingTheSetting()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var exception = Assert.Throws<InvalidOperationException>(
            () => scope.ServiceProvider.GetRequiredService<AppDbContext>());

        Assert.Contains("ConnectionStrings__PhoneBook", exception.Message);
    }

    [Fact]
    public void ResolveDbContext_WithConnectionString_Succeeds()
    {
        using var provider = BuildProvider(
            KeyValuePair.Create<string, string?>("ConnectionStrings:PhoneBook", "Host=localhost;Database=phonebook;Username=u;Password=p"));
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    [Fact]
    public async Task StartAsync_MigrationsDisabled_DoesNotTouchTheDatabase()
    {
        var failingScopeFactory = new ThrowingScopeFactory();
        var migrator = new DatabaseMigrator(
            Options.Create(new DatabaseOptions()),
            new FakeTimeProvider(),
            NullLogger<DatabaseMigrator>.Instance);
        var service = new DatabaseMigrationHostedService(
            failingScopeFactory,
            migrator,
            Options.Create(new DatabaseOptions { ApplyMigrationsOnStartup = false }));

        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.StopAsync(TestContext.Current.CancellationToken);
    }

    private sealed class ThrowingScopeFactory : IServiceScopeFactory
    {
        public IServiceScope CreateScope() => throw new InvalidOperationException("The database must not be touched.");
    }
}
