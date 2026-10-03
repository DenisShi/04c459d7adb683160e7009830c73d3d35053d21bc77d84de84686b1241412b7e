using Microsoft.EntityFrameworkCore;
using PhoneBook.Api.Application;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IPhoneBookDbContext
{
    public DbSet<PhoneNumber> PhoneNumbers => Set<PhoneNumber>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnsureAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private void EnsureAppendOnly()
    {
        var hasMutation = ChangeTracker.Entries<PhoneNumber>()
            .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted);
        if (hasMutation)
        {
            throw new InvalidOperationException("Phone numbers are append-only and cannot be modified or deleted.");
        }
    }
}
