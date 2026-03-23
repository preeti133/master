using MediatR;
using RallyAPI.Orders.Application.DTOs;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Queries.GetOrderById;

/// <summary>
/// Query to get order by ID.
/// </summary>
public sealed record GetOrderByIdQuery(Guid OrderId, Guid CallerId, string CallerRole) : IRequest<Result<OrderDto>>;