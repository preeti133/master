using RallyAPI.SharedKernel.Domain;

namespace RallyAPI.Orders.Domain.Entities;

/// <summary>
/// Cart aggregate — holds items for one customer from one restaurant.
/// One cart per customer at a time (enforced by UNIQUE on customer_id).
/// </summary>
public sealed class Cart : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string RestaurantName { get; private set; } = string.Empty;

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public decimal SubTotal => _items.Sum(i => i.UnitPrice * i.Quantity);
    public int ItemCount => _items.Sum(i => i.Quantity);

    // EF Core constructor
    private Cart() { }

    public static Cart Create(Guid customerId, Guid restaurantId, string restaurantName)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        if (restaurantId == Guid.Empty)
            throw new ArgumentException("Restaurant ID is required", nameof(restaurantId));
        if (string.IsNullOrWhiteSpace(restaurantName))
            throw new ArgumentException("Restaurant name is required", nameof(restaurantName));

        var cart = new Cart();
        cart.Id = Guid.NewGuid();
        cart.CustomerId = customerId;
        cart.RestaurantId = restaurantId;
        cart.RestaurantName = restaurantName.Trim();
        cart.CreatedAt = DateTime.UtcNow;
        cart.UpdatedAt = DateTime.UtcNow;
        return cart;
    }

    /// <summary>
    /// Adds an item to the cart. Items with the same MenuItemId + Options are merged (quantity incremented).
    /// Items with same MenuItemId but different Options are separate line items.
    /// </summary>
    public CartItem AddItem(
        Guid menuItemId,
        string name,
        decimal unitPrice,
        int quantity,
        string? options = null,
        string? specialInstructions = null)
    {
        var existing = _items.FirstOrDefault(i =>
            i.MenuItemId == menuItemId && i.Options == options);

        if (existing != null)
        {
            existing.IncrementQuantity(quantity);
            UpdatedAt = DateTime.UtcNow;
            return existing;
        }

        var item = CartItem.Create(Id, menuItemId, name, unitPrice, quantity, options, specialInstructions);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    /// <summary>
    /// Updates quantity of an existing cart item.
    /// </summary>
    public void UpdateItem(Guid itemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Cart item {itemId} not found in this cart");

        item.SetQuantity(quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a single item from the cart.
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Cart item {itemId} not found in this cart");

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes all items from the cart.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Used by EF Core to populate the navigation collection.
    /// </summary>
    internal void SetItems(IEnumerable<CartItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }
}
