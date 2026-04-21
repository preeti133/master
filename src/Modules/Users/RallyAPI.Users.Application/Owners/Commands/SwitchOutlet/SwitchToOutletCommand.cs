using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Owners.Commands.SwitchOutlet;

public sealed record SwitchToOutletCommand(
    Guid OwnerId,
    Guid RestaurantId) : IRequest<Result<SwitchToOutletResponse>>;

public sealed record SwitchToOutletResponse(
    Guid RestaurantId,
    string Name,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt);
