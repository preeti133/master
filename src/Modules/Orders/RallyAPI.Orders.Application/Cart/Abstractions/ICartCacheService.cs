namespace RallyAPI.Orders.Application.Cart.Abstractions;

public interface ICartCacheService
{
    Task<Domain.Entities.Cart?> GetAsync(Guid customerId, CancellationToken ct = default);
    Task SetAsync(Guid customerId, Domain.Entities.Cart cart, CancellationToken ct = default);
    Task RemoveAsync(Guid customerId, CancellationToken ct = default);
}
