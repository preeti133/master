namespace RallyAPI.SharedKernel.Abstractions.Orders;

/// <summary>
/// Cross-module service for querying escalated orders.
/// Implemented in Orders.Infrastructure, consumed by Users.Endpoints (admin panel).
/// </summary>
public interface IEscalatedOrderQueryService
{
    Task<EscalatedOrdersPagedResult> GetEscalatedOrdersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public sealed record EscalatedOrderSummaryDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string StatusDisplay { get; init; } = string.Empty;
    public string RestaurantName { get; init; } = string.Empty;
    public int TotalItems { get; init; }
    public decimal Total { get; init; }
    public string TotalDisplay { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int? EstimatedMinutes { get; init; }
    public string? EstimatedTimeDisplay { get; init; }
}

public sealed record EscalatedOrdersPagedResult
{
    public IReadOnlyList<EscalatedOrderSummaryDto> Items { get; init; } = Array.Empty<EscalatedOrderSummaryDto>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
