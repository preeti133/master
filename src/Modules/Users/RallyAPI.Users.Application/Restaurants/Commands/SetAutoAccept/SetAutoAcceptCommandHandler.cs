using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Application.Restaurants.Commands.SetAutoAccept;

internal sealed class SetAutoAcceptCommandHandler
    : IRequestHandler<SetAutoAcceptCommand, Result>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetAutoAcceptCommandHandler(
        IRestaurantRepository restaurantRepository,
        IUnitOfWork unitOfWork)
    {
        _restaurantRepository = restaurantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SetAutoAcceptCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(
            request.RestaurantId,
            cancellationToken);

        if (restaurant is null)
            return Result.Failure(Error.NotFound("Restaurant", request.RestaurantId));

        var result = restaurant.SetAutoAcceptOrders(request.AutoAcceptOrders);

        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
