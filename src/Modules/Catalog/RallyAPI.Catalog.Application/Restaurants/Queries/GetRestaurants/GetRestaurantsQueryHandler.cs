// File: src/Modules/Catalog/RallyAPI.Catalog.Application/Restaurants/Queries/GetRestaurants/GetRestaurantsQueryHandler.cs

using MediatR;
using RallyAPI.SharedKernel.Abstractions.Restaurants;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Catalog.Application.Restaurants.Queries.GetRestaurants;

internal sealed class GetRestaurantsQueryHandler
    : IRequestHandler<GetRestaurantsQuery, Result<List<RestaurantListResponse>>>
{
    private readonly IRestaurantQueryService _restaurantQueryService;

    public GetRestaurantsQueryHandler(IRestaurantQueryService restaurantQueryService)
    {
        _restaurantQueryService = restaurantQueryService;
    }

    public async Task<Result<List<RestaurantListResponse>>> Handle(
        GetRestaurantsQuery request,
        CancellationToken cancellationToken)
    {
        var restaurants = await _restaurantQueryService.GetActiveRestaurantsAsync(
            request.Latitude,
            request.Longitude,
            request.RadiusKm,
            cancellationToken);

        var response = restaurants.Select(r => new RestaurantListResponse(
            r.Id,
            r.Name,
            r.AddressLine,
            r.Latitude,
            r.Longitude,
            r.IsAcceptingOrders,
            r.AvgPrepTimeMins,
            r.OpeningTime.ToString("HH:mm"),
            r.ClosingTime.ToString("HH:mm"),
            r.DistanceKm
        )).ToList();

        return response;
    }
}