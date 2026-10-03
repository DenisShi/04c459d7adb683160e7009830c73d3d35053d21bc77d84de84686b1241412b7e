using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Npgsql;

namespace PhoneBook.Api.Infrastructure.Persistence;

public sealed partial class DatabaseMigrator(
    IOptions<DatabaseOptions> options,
    TimeProvider timeProvider,
    ILogger<DatabaseMigrator> logger)
{
    private const string StartingUpSqlState = "57P03";
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(10);

    public async Task MigrateAsync(Func<CancellationToken, Task> migrate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(migrate);
        var timeout = options.Value.MigrationTimeout;
        var startedAt = timeProvider.GetTimestamp();
        var delay = InitialDelay;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                LogAttempt(attempt);
                await migrate(cancellationToken);
                LogUpToDate();
                return;
            }
            catch (Exception exception) when (IsTransient(exception) && !cancellationToken.IsCancellationRequested)
            {
                var elapsed = timeProvider.GetElapsedTime(startedAt);
                if (elapsed + delay > timeout)
                {
                    LogGaveUp(exception, elapsed, attempt);
                    throw;
                }

                LogRetrying(exception, delay);
                await Task.Delay(delay, timeProvider, cancellationToken);
                delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, MaxDelay.Ticks));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogFailed(exception);
                throw;
            }
        }
    }

    public static bool IsTransient(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            var transient = current switch
            {
                PostgresException postgres => postgres.SqlState == StartingUpSqlState,
                NpgsqlException npgsql => npgsql.IsTransient,
                SocketException => true,
                TimeoutException => true,
                _ => false
            };

            if (transient)
            {
                return true;
            }
        }

        return false;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying database migrations, attempt {Attempt}")]
    private partial void LogAttempt(int attempt);

    [LoggerMessage(Level = LogLevel.Information, Message = "Database migrations are up to date")]
    private partial void LogUpToDate();

    [LoggerMessage(Level = LogLevel.Information, Message = "Database is not ready, retrying in {Delay}")]
    private partial void LogRetrying(Exception exception, TimeSpan delay);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Database migrations failed after {Elapsed} and {Attempt} attempts")]
    private partial void LogGaveUp(Exception exception, TimeSpan elapsed, int attempt);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Database migrations failed with a non-transient error")]
    private partial void LogFailed(Exception exception);
}
