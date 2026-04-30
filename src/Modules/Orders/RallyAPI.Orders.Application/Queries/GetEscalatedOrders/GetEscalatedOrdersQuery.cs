using MediatR;
using RallyAPI.Orders.Application.DTOs;
using RallyAPI.Orders.Application.Queries.GetOrdersByCustomer;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Queries.GetEscalatedOrders;

public sealed record GetEscalatedOrdersQuery(
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<OrderSummaryDto>>>;
