using RallyAPI.SharedKernel.Domain;

namespace RallyAPI.Orders.Domain.Entities;

/// <summary>
/// A single line item inside a Cart.
/// </summary>
public sealed class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = "INR";

    /// <summary>
    /// JSON string representing selected options (e.g. size, toppings).
    /// Items with the same MenuItemId but different Options are separate line items.
    /// </summary>
    public string? Options { get; private set; }

    public string? SpecialInstructions { get; private set; }
    public DateTime AddedAt { get; private set; }

    // EF Core constructor
    private CartItem() { }

    public static CartItem Create(
        Guid cartId,
        Guid menuItemId,
        string name,
        decimal unitPrice,
        int quantity,
        string? options = null,
        string? specialInstructions = null)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentException("Cart ID is required", nameof(cartId));
        if (menuItemId == Guid.Empty)
            throw new ArgumentException("Menu item ID is required", nameof(menuItemId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item name is required", nameof(name));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        var item = new CartItem();
        item.Id = Guid.NewGuid();
        item.CartId = cartId;
        item.MenuItemId = menuItemId;
        item.Name = name.Trim();
        item.UnitPrice = unitPrice;
        item.Quantity = quantity;
        item.Options = options;
        item.SpecialInstructions = specialInstructions?.Trim();
        item.AddedAt = DateTime.UtcNow;
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void IncrementQuantity(int by = 1)
    {
        if (by <= 0)
            throw new ArgumentException("Increment must be positive", nameof(by));
        Quantity += by;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
