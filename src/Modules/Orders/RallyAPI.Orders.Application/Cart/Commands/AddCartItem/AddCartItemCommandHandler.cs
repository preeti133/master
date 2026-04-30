using MediatR;
using Microsoft.Extensions.Logging;
using RallyAPI.Orders.Application.Cart.Abstractions;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.Orders.Application.Cart.Mappings;
using RallyAPI.Orders.Application.Abstractions;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, Result<CartDto>>
{
    private readonly ICartRepository _repository;
    private readonly ICartCacheService _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddCartItemCommandHandler> _logger;

    public AddCartItemCommandHandler(
        ICartRepository repository,
        ICartCacheService cache,
        IUnitOfWork unitOfWork,
        ILogger<AddCartItemCommandHandler> logger)
    {
        _repository = repository;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CartDto>> Handle(AddCartItemCommand command, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByCustomerIdAsync(command.CustomerId, cancellationToken);

        if (cart != null && cart.RestaurantId != command.RestaurantId)
        {
            if (!command.ReplaceCart)
            {
                // Return a conflict result with current cart details embedded in error details
                var conflict = new RestaurantConflictDto(
                    cart.RestaurantId,
                    cart.RestaurantName,
                    cart.ItemCount,
                    cart.SubTotal);

                return Result.Failure<CartDto>(Error.Create(
                    "Cart.RestaurantConflict",
                    $"Your cart has items from {cart.RestaurantName}. " +
                    $"Add ?replaceCart=true to clear the cart and start fresh."));
            }

            // Replace: delete old cart
            await _repository.DeleteByCustomerIdAsync(command.CustomerId, cancellationToken);
            await _cache.RemoveAsync(command.CustomerId, cancellationToken);
            cart = null;
        }

        if (cart == null)
        {
            cart = Domain.Entities.Cart.Create(command.CustomerId, command.RestaurantId, command.RestaurantName);
            cart.AddItem(command.MenuItemId, command.Name, command.UnitPrice, command.Quantity, command.Options, command.SpecialInstructions);
            await _repository.CreateAsync(cart, cancellationToken);
        }
        else
        {
            cart.AddItem(command.MenuItemId, command.Name, command.UnitPrice, command.Quantity, command.Options, command.SpecialInstructions);
            await _repository.UpdateAsync(cart, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.SetAsync(command.CustomerId, cart, cancellationToken);

        _logger.LogInformation(
            "Item {MenuItemId} added to cart for customer {CustomerId}",
            command.MenuItemId, command.CustomerId);

        return Result.Success(cart.ToDto());
    }
}
