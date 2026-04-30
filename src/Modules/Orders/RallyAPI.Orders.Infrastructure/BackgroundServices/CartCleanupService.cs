using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RallyAPI.Orders.Application.Cart.Abstractions;

namespace RallyAPI.Orders.Infrastructure.BackgroundServices;

/// <summary>
/// Daily background service that hard-deletes carts not updated in the last 7 days.
/// Runs once per day; uses a scoped service scope for DB access.
/// </summary>
public sealed class CartCleanupService : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromDays(1);
    private static readonly TimeSpan CartTtl = TimeSpan.FromDays(7);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CartCleanupService> _logger;

    public CartCleanupService(IServiceScopeFactory scopeFactory, ILogger<CartCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CartCleanupService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(RunInterval, stoppingToken);

            try
            {
                await CleanupExpiredCartsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cart cleanup");
            }
        }
    }

    private async Task CleanupExpiredCartsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

        var olderThan = DateTime.UtcNow - CartTtl;
        await repository.DeleteExpiredCartsAsync(olderThan, ct);

        _logger.LogInformation("Cart cleanup complete — deleted carts not updated since {OlderThan}", olderThan);
    }
}
