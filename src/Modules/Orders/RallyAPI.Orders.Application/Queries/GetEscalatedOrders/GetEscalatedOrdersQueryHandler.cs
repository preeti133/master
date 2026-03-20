using MediatR;
using RallyAPI.Orders.Application.DTOs;
using RallyAPI.Orders.Application.Mappings;
using RallyAPI.Orders.Application.Queries.GetOrdersByCustomer;
using RallyAPI.Orders.Domain.Abstractions;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Queries.GetEscalatedOrders;

public sealed class GetEscalatedOrdersQueryHandler
    : IRequestHandler<GetEscalatedOrdersQuery, Result<PagedResult<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetEscalatedOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(
        GetEscalatedOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var skip = (query.Page - 1) * query.PageSize;

        var orders = await _orderRepository.GetEscalatedAsync(skip, query.PageSize, cancellationToken);
        var totalCount = await _orderRepository.CountEscalatedAsync(cancellationToken);

        var result = new PagedResult<OrderSummaryDto>
        {
            Items = orders.Select(o => o.ToSummaryDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Result.Success(result);
    }
}
