using MediatR;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand : IRequest<Result<CartDto>>
{
    public required Guid CustomerId { get; init; }
    public required Guid ItemId { get; init; }
    public required int Quantity { get; init; }
}
