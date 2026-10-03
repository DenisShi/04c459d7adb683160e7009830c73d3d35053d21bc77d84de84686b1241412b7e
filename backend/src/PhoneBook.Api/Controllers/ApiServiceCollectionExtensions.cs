using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using PhoneBook.Api.Controllers.OpenApi;

namespace PhoneBook.Api.Controllers;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
                options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider(JsonNamingPolicy.CamelCase)))
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(CreateVisibilityConverter()));

        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(CreateVisibilityConverter()));
        services.AddProblemDetails();
        services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecurityDocumentTransformer>());
        return services;
    }

    private static JsonStringEnumConverter CreateVisibilityConverter() =>
        new(JsonNamingPolicy.SnakeCaseUpper, allowIntegerValues: false);
}
