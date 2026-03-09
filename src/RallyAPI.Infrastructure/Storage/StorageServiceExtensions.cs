using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RallyAPI.SharedKernel.Storage;

namespace RallyAPI.Infrastructure.Storage;

/// <summary>
/// DI registration for storage services.
///
/// USAGE: Call this from RallyAPI.Infrastructure's DependencyInjection.cs:
///   services.AddStorageServices(configuration);
/// </summary>
public static class StorageServiceExtensions
{
    public static IServiceCollection AddStorageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<R2Options>(
            configuration.GetSection(R2Options.SectionName));

        // Register as Singleton — AmazonS3Client is thread-safe and expensive to create
        services.AddSingleton<IStorageService, CloudflareR2StorageService>();

        return services;
    }
}