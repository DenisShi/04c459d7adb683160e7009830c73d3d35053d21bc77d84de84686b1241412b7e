using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PhoneBook.Api.Application;
using PhoneBook.Api.Controllers;
using PhoneBook.Api.Infrastructure.Auth;
using PhoneBook.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddCurrentUser();
builder.Services.AddApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();
app.MapOpenApi("/api/openapi/{documentName}.json").AllowAnonymous();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(PersistenceServiceCollectionExtensions.ReadyHealthCheckTag)
}).AllowAnonymous();
app.MapHealthChecks("/health").AllowAnonymous();

try
{
    await app.RunAsync();
    return 0;
}
catch (Exception exception) when (exception is not OperationCanceledException)
{
    return 1;
}

public partial class Program;
