using MediatR;
using Microsoft.Extensions.Logging;
using RallyAPI.Orders.Application.Cart.Abstractions;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.Orders.Application.Cart.Mappings;
using RallyAPI.Orders.Application.Abstractions;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.UpdateCartItem;

public sealed class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, Result<CartDto>>
{
    private readonly ICartRepository _repository;
    private readonly ICartCacheService _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCartItemCommandHandler> _logger;

    public UpdateCartItemCommandHandler(
        ICartRepository repository,
        ICartCacheService cache,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCartItemCommandHandler> logger)
    {
        _repository = repository;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByCustomerIdAsync(command.CustomerId, cancellationToken);
        if (cart == null)
            return Result.Failure<CartDto>(Error.Create("Cart.NotFound", "Cart not found"));

        var item = cart.Items.FirstOrDefault(i => i.Id == command.ItemId);
        if (item == null)
            return Result.Failure<CartDto>(Error.Create("Cart.ItemNotFound", $"Cart item {command.ItemId} not found"));

        cart.UpdateItem(command.ItemId, command.Quantity);
        await _repository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.SetAsync(command.CustomerId, cart, cancellationToken);

        return Result.Success(cart.ToDto());
    }
}
