using MediatR;
using Microsoft.Extensions.Logging;
using RallyAPI.Orders.Application.Cart.Abstractions;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.Orders.Application.Cart.Mappings;
using RallyAPI.Orders.Application.Abstractions;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, Result<CartDto?>>
{
    private readonly ICartRepository _repository;
    private readonly ICartCacheService _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveCartItemCommandHandler> _logger;

    public RemoveCartItemCommandHandler(
        ICartRepository repository,
        ICartCacheService cache,
        IUnitOfWork unitOfWork,
        ILogger<RemoveCartItemCommandHandler> logger)
    {
        _repository = repository;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CartDto?>> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByCustomerIdAsync(command.CustomerId, cancellationToken);
        if (cart == null)
            return Result.Failure<CartDto?>(Error.Create("Cart.NotFound", "Cart not found"));

        var item = cart.Items.FirstOrDefault(i => i.Id == command.ItemId);
        if (item == null)
            return Result.Failure<CartDto?>(Error.Create("Cart.ItemNotFound", $"Cart item {command.ItemId} not found"));

        cart.RemoveItem(command.ItemId);

        if (!cart.Items.Any())
        {
            // Cart is now empty — delete it entirely.
            // ExecuteDeleteAsync runs its own SQL DELETE and bypasses the change tracker,
            // so we must NOT call SaveChangesAsync afterwards (the cart row is already gone
            // and EF would try to update/delete it a second time causing a concurrency error).
            await _cache.RemoveAsync(command.CustomerId, cancellationToken);
            await _repository.DeleteByCustomerIdAsync(command.CustomerId, cancellationToken);
            return Result.Success<CartDto?>(null);
        }

        await _repository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.SetAsync(command.CustomerId, cart, cancellationToken);

        return Result.Success<CartDto?>(cart.ToDto());
    }
}
