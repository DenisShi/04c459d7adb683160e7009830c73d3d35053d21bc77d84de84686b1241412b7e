using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using PhoneBook.Api.Application;
using PhoneBook.Api.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PhoneBook.Api.IntegrationTests;

public sealed class ApiFixture : IAsyncLifetime
{
    public const string PostgresImage = "postgres:18.6-alpine3.24";

    private readonly PostgreSqlContainer container = new PostgreSqlBuilder(PostgresImage).Build();

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 10, 3, 12, 0, 0, TimeSpan.Zero));

    public ApiFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await container.StartAsync();
        Factory = new ApiFactory(container.GetConnectionString(), Clock);
        _ = Factory.Server;
    }

    public async ValueTask DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await container.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE phone_numbers");
    }

    public HttpClient CreateClient(string subject, string? username = null)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add(HeaderCurrentUser.SubjectHeader, subject);
        client.DefaultRequestHeaders.Add(HeaderCurrentUser.UsernameHeader, username ?? subject);
        return client;
    }
}

public sealed class ApiFactory(string connectionString, TimeProvider timeProvider) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PhoneBook"] = connectionString
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton(timeProvider);
            services.RemoveAll<ICurrentUser>();
            services.AddScoped<ICurrentUser, HeaderCurrentUser>();
        });
    }
}

[CollectionDefinition(Name)]
public sealed class ApiTestGroup : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
