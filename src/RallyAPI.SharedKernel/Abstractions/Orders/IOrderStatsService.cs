namespace RallyAPI.SharedKernel.Abstractions.Orders;

/// <summary>
/// Cross-module service for order aggregate counts.
/// Implemented in Orders.Infrastructure, consumed by Users.Application (admin stats).
/// </summary>
public interface IOrderStatsService
{
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTodayCountAsync(CancellationToken cancellationToken = default);
}
