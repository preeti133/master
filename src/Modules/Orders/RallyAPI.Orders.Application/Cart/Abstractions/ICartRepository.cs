using RallyAPI.Orders.Domain.Entities;

namespace RallyAPI.Orders.Application.Cart.Abstractions;

public interface ICartRepository
{
    Task<Domain.Entities.Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task CreateAsync(Domain.Entities.Cart cart, CancellationToken ct = default);
    Task UpdateAsync(Domain.Entities.Cart cart, CancellationToken ct = default);
    Task DeleteByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task DeleteExpiredCartsAsync(DateTime olderThan, CancellationToken ct = default);
}
