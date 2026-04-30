using MediatR;
using Microsoft.Extensions.Logging;
using RallyAPI.Orders.Application.Cart.Abstractions;
using RallyAPI.Orders.Application.Abstractions;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.ClearCart;

public sealed class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Result>
{
    private readonly ICartRepository _repository;
    private readonly ICartCacheService _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearCartCommandHandler> _logger;

    public ClearCartCommandHandler(
        ICartRepository repository,
        ICartCacheService cache,
        IUnitOfWork unitOfWork,
        ILogger<ClearCartCommandHandler> logger)
    {
        _repository = repository;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ClearCartCommand command, CancellationToken cancellationToken)
    {
        await _repository.DeleteByCustomerIdAsync(command.CustomerId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(command.CustomerId, cancellationToken);

        _logger.LogInformation("Cart cleared for customer {CustomerId}", command.CustomerId);
        return Result.Success();
    }
}
