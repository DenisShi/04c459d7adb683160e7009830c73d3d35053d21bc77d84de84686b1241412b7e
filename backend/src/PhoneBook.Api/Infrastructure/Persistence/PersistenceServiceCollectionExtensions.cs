using Microsoft.EntityFrameworkCore;
using PhoneBook.Api.Application;

namespace PhoneBook.Api.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public const string ConnectionStringName = "PhoneBook";
    public const string ReadyHealthCheckTag = "ready";

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<AppDbContext>((provider, builder) =>
        {
            var connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"The connection string '{ConnectionStringName}' is not configured. Set ConnectionStrings__{ConnectionStringName}.");
            }

            builder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IPhoneBookDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddSingleton<DatabaseMigrator>();
        services.AddHostedService<DatabaseMigrationHostedService>();
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database", tags: [ReadyHealthCheckTag]);
        return services;
    }
}
