using MediatR;
using Microsoft.Extensions.Logging;
using RallyAPI.Orders.Application.Cart.Abstractions;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.Orders.Application.Cart.Mappings;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Queries.GetCart;

public sealed class GetCartQueryHandler : IRequestHandler<GetCartQuery, Result<CartDto?>>
{
    private readonly ICartRepository _repository;
    private readonly ICartCacheService _cache;
    private readonly ILogger<GetCartQueryHandler> _logger;

    public GetCartQueryHandler(
        ICartRepository repository,
        ICartCacheService cache,
        ILogger<GetCartQueryHandler> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<CartDto?>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        // 1. Try cache first
        var cached = await _cache.GetAsync(request.CustomerId, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cart cache hit for customer {CustomerId}", request.CustomerId);
            return Result.Success<CartDto?>(cached.ToDto());
        }

        // 2. Cache miss — load from DB
        var cart = await _repository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        if (cart == null)
            return Result.Success<CartDto?>(null);

        // 3. Backfill cache
        await _cache.SetAsync(request.CustomerId, cart, cancellationToken);

        return Result.Success<CartDto?>(cart.ToDto());
    }
}
