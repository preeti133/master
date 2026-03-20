using Microsoft.EntityFrameworkCore;
using RallyAPI.Orders.Domain.Enums;
using RallyAPI.SharedKernel.Abstractions.Orders;

namespace RallyAPI.Orders.Infrastructure.Services;

public sealed class OrderStatsService : IOrderStatsService
{
    private static readonly OrderStatus[] ActiveStatuses =
    [
        OrderStatus.Paid,
        OrderStatus.Confirmed,
        OrderStatus.Preparing,
        OrderStatus.ReadyForPickup,
        OrderStatus.PickedUp
    ];

    private readonly OrdersDbContext _context;

    public OrderStatsService(OrdersDbContext context)
    {
        _context = context;
    }

    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        => _context.Orders.CountAsync(cancellationToken);

    public Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
        => _context.Orders.CountAsync(o => ActiveStatuses.Contains(o.Status), cancellationToken);

    public Task<int> GetTodayCountAsync(CancellationToken cancellationToken = default)
    {
        var startOfDay = DateTime.UtcNow.Date;
        return _context.Orders.CountAsync(o => o.CreatedAt >= startOfDay, cancellationToken);
    }
}
