using WineApi.Service;

namespace WineApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<AuthService>();
        services.AddScoped<WineService>();

        return services;
    }
}