using MediatR;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.SyncCart;

public record SyncCartItemDto(
    Guid MenuItemId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    string? Options,
    string? SpecialInstructions);

public record SyncCartCommand : IRequest<Result<CartDto>>
{
    public required Guid CustomerId { get; init; }
    public required Guid RestaurantId { get; init; }
    public required string RestaurantName { get; init; }
    public required List<SyncCartItemDto> Items { get; init; }

    /// <summary>
    /// When true, replaces any existing cart from a different restaurant.
    /// </summary>
    public bool ReplaceCart { get; init; }
}
