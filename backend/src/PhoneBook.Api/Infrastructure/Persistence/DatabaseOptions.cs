using System.ComponentModel.DataAnnotations;

namespace PhoneBook.Api.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool ApplyMigrationsOnStartup { get; init; } = true;

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan MigrationTimeout { get; init; } = TimeSpan.FromSeconds(90);
}
