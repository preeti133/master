using Microsoft.EntityFrameworkCore;
using RallyAPI.Orders.Domain.Enums;
using RallyAPI.SharedKernel.Abstractions.Orders;

namespace RallyAPI.Orders.Infrastructure.Services;

public sealed class EscalatedOrderQueryService : IEscalatedOrderQueryService
{
    private readonly OrdersDbContext _context;

    public EscalatedOrderQueryService(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<EscalatedOrdersPagedResult> GetEscalatedOrdersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;

        var baseQuery = _context.Orders
            .AsNoTracking()
            .Where(o => o.IsEscalated)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var orders = await baseQuery
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = orders.Select(o => new EscalatedOrderSummaryDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber.Value,
            Status = o.Status.ToString(),
            StatusDisplay = o.Status.GetDisplayName(),
            RestaurantName = o.RestaurantName,
            TotalItems = o.Items.Sum(i => i.Quantity),
            Total = o.Pricing.Total.Amount,
            TotalDisplay = o.Pricing.Total.ToDisplayString(),
            CreatedAt = o.CreatedAt,
            EstimatedMinutes = o.DeliveryInfo.EstimatedMinutes,
            EstimatedTimeDisplay = o.DeliveryInfo.EstimatedMinutes.HasValue
                ? $"{o.DeliveryInfo.EstimatedMinutes} mins"
                : null
        }).ToList();

        return new EscalatedOrdersPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
