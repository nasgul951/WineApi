using WineApi.Service;

namespace WineApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWineService, WineService>();

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        // Register CORS policy
        var corsOrigins = config.GetCorsOrigins("http://localhost:3000");
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins(corsOrigins.ToArray())
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        return services;
    }
}