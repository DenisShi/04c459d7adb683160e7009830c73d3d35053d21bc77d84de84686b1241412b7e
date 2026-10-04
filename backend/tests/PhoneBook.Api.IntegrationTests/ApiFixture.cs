using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using PhoneBook.Api.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PhoneBook.Api.IntegrationTests;

public sealed class ApiFixture : IAsyncLifetime
{
    public const string PostgresImage = "postgres:18.6-alpine3.24";

    private readonly PostgreSqlContainer container = new PostgreSqlBuilder(PostgresImage).Build();

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 10, 3, 12, 0, 0, TimeSpan.Zero));

    public TestTokens Tokens { get; } = new();

    public ApiFactory Factory { get; private set; } = null!;

    public JwtApiFactory JwtFactory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await container.StartAsync();
        Factory = new ApiFactory(container.GetConnectionString(), Clock);
        _ = Factory.Server;
        JwtFactory = new JwtApiFactory(container.GetConnectionString(), Tokens.Configuration);
        _ = JwtFactory.Server;
    }

    public async ValueTask DisposeAsync()
    {
        if (JwtFactory is not null)
        {
            await JwtFactory.DisposeAsync();
        }

        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await container.DisposeAsync();
        Tokens.Dispose();
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
        client.DefaultRequestHeaders.Add(TestAuthHandler.SubjectHeader, subject);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UsernameHeader, username ?? subject);
        return client;
    }

    public HttpClient CreateJwtClient(string? token)
    {
        var client = JwtFactory.CreateClient();
        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }
}

public abstract class PhoneBookApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PhoneBook"] = connectionString,
                ["Keycloak:MetadataAddress"] = $"{TestTokens.Issuer}/.well-known/openid-configuration",
                ["Keycloak:ValidIssuer"] = TestTokens.Issuer,
                ["Keycloak:Audience"] = TestTokens.Audience,
                ["Keycloak:RequireHttpsMetadata"] = "true"
            }));

        builder.ConfigureTestServices(ConfigureTestServices);
    }

    protected abstract void ConfigureTestServices(IServiceCollection services);
}

public sealed class ApiFactory(string connectionString, TimeProvider timeProvider)
    : PhoneBookApiFactory(connectionString)
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.RemoveAll<TimeProvider>();
        services.AddSingleton(timeProvider);
        services.AddAuthentication(TestAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, null);
    }
}

public sealed class JwtApiFactory(string connectionString, OpenIdConnectConfiguration configuration)
    : PhoneBookApiFactory(connectionString)
{
    protected override void ConfigureTestServices(IServiceCollection services) =>
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Configuration = configuration;
            options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration);
        });
}

[CollectionDefinition(Name)]
public sealed class ApiTestGroup : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
