using MediatR;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Queries.GetCart;

public record GetCartQuery(Guid CustomerId) : IRequest<Result<CartDto?>>;
