using MediatR;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand : IRequest<Result<CartDto?>>
{
    public required Guid CustomerId { get; init; }
    public required Guid ItemId { get; init; }
}
