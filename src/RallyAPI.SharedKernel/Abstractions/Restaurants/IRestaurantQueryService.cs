// File: src/RallyAPI.SharedKernel/Abstractions/Restaurants/IRestaurantQueryService.cs
// Purpose: Cross-module abstraction for querying restaurant data.
//          Implemented by Users module, consumed by Catalog module.

namespace RallyAPI.SharedKernel.Abstractions.Restaurants;

public interface IRestaurantQueryService
{
    /// <summary>
    /// Gets active restaurants, optionally filtered by proximity to a location.
    /// Results are sorted by distance if lat/lng provided, otherwise by name.
    /// </summary>
    Task<IReadOnlyList<RestaurantSummary>> GetActiveRestaurantsAsync(
        double? latitude = null,
        double? longitude = null,
        double? radiusKm = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a single restaurant's details by ID.
    /// </summary>
    Task<RestaurantDetails?> GetByIdAsync(
        Guid restaurantId,
        CancellationToken ct = default);
}
