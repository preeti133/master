using System.Text.Json;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.Orders.Domain.Entities;

namespace RallyAPI.Orders.Application.Cart.Mappings;

public static class CartMappingExtensions
{
    public static CartDto ToDto(this Domain.Entities.Cart cart) => new(
        cart.Id,
        cart.CustomerId,
        cart.RestaurantId,
        cart.RestaurantName,
        cart.Items.Select(i => i.ToDto()).ToList(),
        cart.SubTotal,
        cart.ItemCount,
        cart.UpdatedAt);

    public static CartItemDto ToDto(this CartItem item) => new(
        item.Id,
        item.MenuItemId,
        item.Name,
        item.Quantity,
        item.UnitPrice,
        item.UnitPrice * item.Quantity,
        item.Currency,
        ParseOptions(item.Options),
        item.SpecialInstructions);

    private static List<CartItemOptionDto>? ParseOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<CartItemOptionDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }
}
