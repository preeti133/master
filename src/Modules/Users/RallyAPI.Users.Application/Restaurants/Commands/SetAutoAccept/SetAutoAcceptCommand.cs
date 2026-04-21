using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Restaurants.Commands.SetAutoAccept;

public sealed record SetAutoAcceptCommand(
    Guid RestaurantId,
    bool AutoAcceptOrders) : IRequest<Result>;
