using PhoneBook.Api.Application;

namespace PhoneBook.Api.Infrastructure.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}
