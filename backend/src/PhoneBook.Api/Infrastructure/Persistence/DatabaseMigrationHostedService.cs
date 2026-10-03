using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace PhoneBook.Api.Infrastructure.Persistence;

public sealed class DatabaseMigrationHostedService(
    IServiceScopeFactory scopeFactory,
    DatabaseMigrator migrator,
    IOptions<DatabaseOptions> options) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.ApplyMigrationsOnStartup)
        {
            return;
        }

        await migrator.MigrateAsync(MigrateAsync, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
