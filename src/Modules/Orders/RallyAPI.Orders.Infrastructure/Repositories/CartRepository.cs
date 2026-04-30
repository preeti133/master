using Microsoft.EntityFrameworkCore;
using RallyAPI.Orders.Application.Cart.Abstractions;
using RallyAPI.Orders.Domain.Entities;

namespace RallyAPI.Orders.Infrastructure.Repositories;

public sealed class CartRepository : ICartRepository
{
    private readonly OrdersDbContext _context;

    public CartRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
    }

    public async Task CreateAsync(Cart cart, CancellationToken ct = default)
    {
        await _context.Carts.AddAsync(cart, ct);
    }

    public Task UpdateAsync(Cart cart, CancellationToken ct = default)
    {
        _context.Carts.Update(cart);
        return Task.CompletedTask;
    }

    public async Task DeleteByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        await _context.Carts
            .Where(c => c.CustomerId == customerId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteExpiredCartsAsync(DateTime olderThan, CancellationToken ct = default)
    {
        await _context.Carts
            .Where(c => c.UpdatedAt < olderThan)
            .ExecuteDeleteAsync(ct);
    }
}
