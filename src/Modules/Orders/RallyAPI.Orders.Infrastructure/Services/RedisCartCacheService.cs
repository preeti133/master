using System.Text.Json;
using Microsoft.Extensions.Logging;
using RallyAPI.Orders.Application.Cart.Abstractions;
using RallyAPI.Orders.Domain.Entities;
using StackExchange.Redis;

namespace RallyAPI.Orders.Infrastructure.Services;

/// <summary>
/// Write-through Redis cache for carts.
/// Key: cart:{customerId}  TTL: 7 days (reset on every write).
/// </summary>
public sealed class RedisCartCacheService : ICartCacheService
{
    private const int TtlSeconds = 604800; // 7 days
    private readonly IDatabase _redis;
    private readonly ILogger<RedisCartCacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCartCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCartCacheService> logger)
    {
        _redis = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    public async Task<Cart?> GetAsync(Guid customerId, CancellationToken ct = default)
    {
        try
        {
            var key = CacheKey(customerId);
            var json = await _redis.StringGetAsync(key);
            if (json.IsNullOrEmpty)
                return null;

            return Deserialize(json!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis read failed for cart:{CustomerId} — falling back to DB", customerId);
            return null;
        }
    }

    public async Task SetAsync(Guid customerId, Cart cart, CancellationToken ct = default)
    {
        try
        {
            var key = CacheKey(customerId);
            var json = Serialize(cart);
            await _redis.StringSetAsync(key, json, TimeSpan.FromSeconds(TtlSeconds));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis write failed for cart:{CustomerId}", customerId);
        }
    }

    public async Task RemoveAsync(Guid customerId, CancellationToken ct = default)
    {
        try
        {
            await _redis.KeyDeleteAsync(CacheKey(customerId));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis delete failed for cart:{CustomerId}", customerId);
        }
    }

    private static string CacheKey(Guid customerId) => $"cart:{customerId}";

    private static string Serialize(Cart cart)
    {
        var dto = new CartCacheDto
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            RestaurantId = cart.RestaurantId,
            RestaurantName = cart.RestaurantName,
            UpdatedAt = cart.UpdatedAt,
            CreatedAt = cart.CreatedAt,
            Items = cart.Items.Select(i => new CartItemCacheDto
            {
                Id = i.Id,
                CartId = i.CartId,
                MenuItemId = i.MenuItemId,
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Currency = i.Currency,
                Options = i.Options,
                SpecialInstructions = i.SpecialInstructions,
                AddedAt = i.AddedAt,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList()
        };
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    private static Cart? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<CartCacheDto>(json, JsonOptions);
        if (dto == null) return null;

        var cart = Cart.Create(dto.CustomerId, dto.RestaurantId, dto.RestaurantName);

        // Reconstruct items via domain method
        foreach (var itemDto in dto.Items)
        {
            cart.AddItem(itemDto.MenuItemId, itemDto.Name, itemDto.UnitPrice,
                itemDto.Quantity, itemDto.Options, itemDto.SpecialInstructions);
        }

        return cart;
    }

    // Private DTOs for cache serialization — avoids serializing EF-tracked entities
    private sealed class CartCacheDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemCacheDto> Items { get; set; } = new();
    }

    private sealed class CartItemCacheDto
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = "INR";
        public string? Options { get; set; }
        public string? SpecialInstructions { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
