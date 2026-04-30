using MediatR;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.AddCartItem;

public record AddCartItemCommand : IRequest<Result<CartDto>>
{
    public required Guid CustomerId { get; init; }
    public required Guid RestaurantId { get; init; }
    public required string RestaurantName { get; init; }
    public required Guid MenuItemId { get; init; }
    public required string Name { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int Quantity { get; init; }

    /// <summary>
    /// JSON-serialized list of selected options (e.g. size, toppings).
    /// Items with the same MenuItemId but different Options are separate line items.
    /// </summary>
    public string? Options { get; init; }

    public string? SpecialInstructions { get; init; }

    /// <summary>
    /// When true, clears the existing cart (from a different restaurant) and starts fresh.
    /// </summary>
    public bool ReplaceCart { get; init; }
}
