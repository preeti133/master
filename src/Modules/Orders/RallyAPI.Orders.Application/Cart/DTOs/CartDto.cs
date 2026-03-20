namespace RallyAPI.Orders.Application.Cart.DTOs;

public record CartItemOptionDto(string Name, string Value);

public record CartItemDto(
    Guid Id,
    Guid MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string Currency,
    List<CartItemOptionDto>? Options,
    string? SpecialInstructions);

public record CartDto(
    Guid Id,
    Guid CustomerId,
    Guid RestaurantId,
    string RestaurantName,
    List<CartItemDto> Items,
    decimal SubTotal,
    int ItemCount,
    DateTime UpdatedAt);

/// <summary>
/// Returned in the 409 conflict response body when the customer has items
/// from a different restaurant.
/// </summary>
public record RestaurantConflictDto(
    Guid RestaurantId,
    string RestaurantName,
    int ItemCount,
    decimal SubTotal);
