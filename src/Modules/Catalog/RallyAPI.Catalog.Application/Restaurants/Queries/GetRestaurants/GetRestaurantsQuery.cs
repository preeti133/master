// File: src/Modules/Catalog/RallyAPI.Catalog.Application/Restaurants/Queries/GetRestaurants/GetRestaurantsQuery.cs

using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Catalog.Application.Restaurants.Queries.GetRestaurants;

public sealed record GetRestaurantsQuery(
    double? Latitude = null,
    double? Longitude = null,
    double? RadiusKm = null
) : IRequest<Result<List<RestaurantListResponse>>>;

public sealed record RestaurantListResponse(
    Guid Id,
    string Name,
    string AddressLine,
    double Latitude,
    double Longitude,
    bool IsAcceptingOrders,
    int AvgPrepTimeMins,
    string OpeningTime,
    string ClosingTime,
    double? DistanceKm);