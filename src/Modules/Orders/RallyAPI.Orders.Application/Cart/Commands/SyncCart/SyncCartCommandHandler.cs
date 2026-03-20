using MediatR;
using Microsoft.Extensions.Logging;
using RallyAPI.Orders.Application.Cart.Abstractions;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.Orders.Application.Cart.Mappings;
using RallyAPI.Orders.Application.Abstractions;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.SyncCart;

/// <summary>
/// Merges a guest cart (built before login) into the authenticated customer's server-side cart.
///
/// Merge rules:
///   - No server cart → create from guest items
///   - Same restaurant → merge: increment matching items (same MenuItemId + Options), add new ones
///   - Different restaurant + ReplaceCart=false → 409 conflict
///   - Different restaurant + ReplaceCart=true  → replace server cart with guest items
/// </summary>
public sealed class SyncCartCommandHandler : IRequestHandler<SyncCartCommand, Result<CartDto>>
{
    private readonly ICartRepository _repository;
    private readonly ICartCacheService _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyncCartCommandHandler> _logger;

    public SyncCartCommandHandler(
        ICartRepository repository,
        ICartCacheService cache,
        IUnitOfWork unitOfWork,
        ILogger<SyncCartCommandHandler> logger)
    {
        _repository = repository;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CartDto>> Handle(SyncCartCommand command, CancellationToken cancellationToken)
    {
        if (!command.Items.Any())
        {
            // Nothing to sync — return existing server cart (or empty)
            var existing = await _repository.GetByCustomerIdAsync(command.CustomerId, cancellationToken);
            if (existing == null)
                return Result.Failure<CartDto>(Error.Create("Cart.Empty", "No items to sync"));

            return Result.Success(existing.ToDto());
        }

        var serverCart = await _repository.GetByCustomerIdAsync(command.CustomerId, cancellationToken);

        if (serverCart != null && serverCart.RestaurantId != command.RestaurantId)
        {
            if (!command.ReplaceCart)
            {
                return Result.Failure<CartDto>(Error.Create(
                    "Cart.RestaurantConflict",
                    $"Your cart has items from {serverCart.RestaurantName}. " +
                    $"Add ?replaceCart=true to clear the cart and start fresh."));
            }

            // Replace existing cart
            await _repository.DeleteByCustomerIdAsync(command.CustomerId, cancellationToken);
            await _cache.RemoveAsync(command.CustomerId, cancellationToken);
            serverCart = null;
        }

        if (serverCart == null)
        {
            // Create new cart from guest items
            serverCart = Domain.Entities.Cart.Create(command.CustomerId, command.RestaurantId, command.RestaurantName);
            foreach (var guestItem in command.Items)
            {
                serverCart.AddItem(guestItem.MenuItemId, guestItem.Name, guestItem.UnitPrice,
                    guestItem.Quantity, guestItem.Options, guestItem.SpecialInstructions);
            }
            await _repository.CreateAsync(serverCart, cancellationToken);
        }
        else
        {
            // Merge into existing same-restaurant cart
            foreach (var guestItem in command.Items)
            {
                serverCart.AddItem(guestItem.MenuItemId, guestItem.Name, guestItem.UnitPrice,
                    guestItem.Quantity, guestItem.Options, guestItem.SpecialInstructions);
            }
            await _repository.UpdateAsync(serverCart, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.SetAsync(command.CustomerId, serverCart, cancellationToken);

        _logger.LogInformation(
            "Synced guest cart ({ItemCount} items) for customer {CustomerId}",
            command.Items.Count, command.CustomerId);

        return Result.Success(serverCart.ToDto());
    }
}
