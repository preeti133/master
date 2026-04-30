using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Application.Owners.Queries.GetOutlets;

internal sealed class GetOwnerOutletsQueryHandler
    : IRequestHandler<GetOwnerOutletsQuery, Result<IReadOnlyList<OutletSummaryResponse>>>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public GetOwnerOutletsQueryHandler(IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Result<IReadOnlyList<OutletSummaryResponse>>> Handle(
        GetOwnerOutletsQuery request,
        CancellationToken cancellationToken)
    {
        var outlets = await _restaurantRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);

        var response = outlets
            .Select(r => new OutletSummaryResponse(
                r.Id,
                r.Name,
                r.Email.Value,
                r.AddressLine,
                r.IsActive,
                r.IsAcceptingOrders,
                r.LogoUrl))
            .ToList();

        return Result.Success<IReadOnlyList<OutletSummaryResponse>>(response);
    }
}
