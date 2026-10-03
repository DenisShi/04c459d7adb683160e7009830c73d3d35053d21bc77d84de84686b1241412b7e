using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PhoneBook.Api.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<PhoneNumberService>();
        return services;
    }
}
