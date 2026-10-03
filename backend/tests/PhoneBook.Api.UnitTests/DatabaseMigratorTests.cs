using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using PhoneBook.Api.Infrastructure.Persistence;

namespace PhoneBook.Api.UnitTests;

public class DatabaseMigratorTests
{
    private static DatabaseMigrator CreateMigrator(FakeTimeProvider clock, TimeSpan timeout) =>
        new(
            Options.Create(new DatabaseOptions { MigrationTimeout = timeout }),
            clock,
            NullLogger<DatabaseMigrator>.Instance);

    private static async Task RunWithClock(Task task, FakeTimeProvider clock)
    {
        while (!task.IsCompleted)
        {
            clock.Advance(TimeSpan.FromSeconds(1));
            await Task.Yield();
        }

        await task;
    }

    private static NpgsqlException TransientNpgsqlException() =>
        new("connection failed", new SocketException());

    [Fact]
    public async Task MigrateAsync_SucceedsImmediately_RunsOnce()
    {
        var clock = new FakeTimeProvider();
        var calls = 0;

        await CreateMigrator(clock, TimeSpan.FromSeconds(30)).MigrateAsync(
            _ =>
            {
                calls++;
                return Task.CompletedTask;
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task MigrateAsync_TransientFailuresThenSuccess_Retries()
    {
        var clock = new FakeTimeProvider();
        var calls = 0;

        var task = CreateMigrator(clock, TimeSpan.FromSeconds(60)).MigrateAsync(
            _ =>
            {
                calls++;
                return calls < 3 ? Task.FromException(TransientNpgsqlException()) : Task.CompletedTask;
            },
            TestContext.Current.CancellationToken);
        await RunWithClock(task, clock);

        Assert.Equal(3, calls);
    }

    [Fact]
    public async Task MigrateAsync_TransientFailureBeyondTimeout_ThrowsLastException()
    {
        var clock = new FakeTimeProvider();
        var calls = 0;

        var task = CreateMigrator(clock, TimeSpan.FromSeconds(5)).MigrateAsync(
            _ =>
            {
                calls++;
                return Task.FromException(TransientNpgsqlException());
            },
            TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<NpgsqlException>(() => RunWithClock(task, clock));
        Assert.True(calls > 1);
    }

    [Fact]
    public async Task MigrateAsync_NonTransientFailure_ThrowsWithoutRetry()
    {
        var clock = new FakeTimeProvider();
        var calls = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateMigrator(clock, TimeSpan.FromSeconds(30)).MigrateAsync(
            _ =>
            {
                calls++;
                return Task.FromException(new InvalidOperationException("broken migration"));
            },
            TestContext.Current.CancellationToken));

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task MigrateAsync_CancelledWhileWaiting_StopsRetrying()
    {
        var clock = new FakeTimeProvider();
        using var cancellation = new CancellationTokenSource();
        var calls = 0;

        var task = CreateMigrator(clock, TimeSpan.FromMinutes(5)).MigrateAsync(
            _ =>
            {
                calls++;
                cancellation.Cancel();
                return Task.FromException(TransientNpgsqlException());
            },
            cancellation.Token);

        await Assert.ThrowsAsync<NpgsqlException>(() => task);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void IsTransient_StartingUpPostgresError_ReturnsTrue()
    {
        var exception = new PostgresException("starting up", "FATAL", "FATAL", "57P03");

        Assert.True(DatabaseMigrator.IsTransient(exception));
    }

    [Fact]
    public void IsTransient_OtherPostgresError_ReturnsFalse()
    {
        var exception = new PostgresException("password authentication failed", "FATAL", "FATAL", "28P01");

        Assert.False(DatabaseMigrator.IsTransient(exception));
    }

    [Fact]
    public void IsTransient_NpgsqlExceptionWithSocketCause_ReturnsTrue()
    {
        Assert.True(DatabaseMigrator.IsTransient(TransientNpgsqlException()));
    }

    [Fact]
    public void IsTransient_WrappedSocketException_ReturnsTrue()
    {
        Assert.True(DatabaseMigrator.IsTransient(new InvalidOperationException("wrapper", new SocketException())));
    }

    [Fact]
    public void IsTransient_TimeoutException_ReturnsTrue()
    {
        Assert.True(DatabaseMigrator.IsTransient(new TimeoutException()));
    }

    [Fact]
    public void IsTransient_UnrelatedException_ReturnsFalse()
    {
        Assert.False(DatabaseMigrator.IsTransient(new InvalidOperationException()));
    }
}
