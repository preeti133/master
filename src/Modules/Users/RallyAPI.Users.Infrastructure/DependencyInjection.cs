using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RallyAPI.Users.Application.Abstractions;
using RallyAPI.Users.Infrastructure.Persistence;
using RallyAPI.Users.Infrastructure.Persistence.Repositories;
using RallyAPI.Users.Infrastructure.Services;
using RallyAPI.SharedKernel.Abstractions.Riders;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StackExchange.Redis;
using RallyAPI.SharedKernel.Abstractions.Restaurants;
namespace RallyAPI.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDatabase(configuration);

        // Repositories
        services.AddRepositories();

        // Services
        services.AddServices(configuration);

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        //services.AddDbContext<UsersDbContext>(options =>
        //{
        //    options.UseNpgsql(connectionString, npgsqlOptions =>
        //    {
        //        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
        //    });
        //});

        services.AddDbContext<UsersDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
            });

            options.AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>());
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IRiderRepository, RiderRepository>();
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRestaurantQueryService, RestaurantQueryService>();


        // Rider services for cross-module communication
        services.AddScoped<IRiderQueryService, RiderQueryService>();
        services.AddScoped<IRiderCommandService, RiderCommandService>();

        return services;
    }

    private static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Settings
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        // Services
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        // Redis
        var redisConnection = configuration.GetConnectionString("Redis")!;
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));

        // SMS Service — Exotel in production, Console in development

        var useExotel = configuration.GetSection("Exotel").Exists();
        if (useExotel)
        {
            services.Configure<ExotelOptions>(
                configuration.GetSection(ExotelOptions.SectionName));

            services.AddHttpClient<ISmsService, ExotelSmsService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });
        }
        else
        {
            services.AddSingleton<ISmsService, ConsoleSmsService>();
        }

        // -------------------------------------------------------
        // NOTE ON AddHttpClient<ISmsService, ExotelSmsService>:
        // -------------------------------------------------------
        // This registers ExotelSmsService as a TYPED HttpClient, meaning:
        //   - HttpClient is injected into ExotelSmsService's constructor
        //   - HttpClient lifecycle is managed by IHttpClientFactory (pooling, DNS refresh)
        //   - No need to manually create/dispose HttpClient
        //
        // If your DI method uses WebApplicationBuilder:
        //   Replace `services.` with `builder.Services.`
        //   Replace `configuration.` with `builder.Configuration.`
        //
        // If your DI method uses IServiceCollection + IConfiguration params:
        //   Use them directly as shown above.
        // ============================================================================

        services.AddScoped<IOtpService, OtpService>();

        return services;
    }
}