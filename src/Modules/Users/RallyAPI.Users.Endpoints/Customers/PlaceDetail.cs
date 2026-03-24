using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.SharedKernel.Abstractions.Geocoding;

namespace RallyAPI.Users.Endpoints.Customers;

public class PlaceDetail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/places/{placeId}", HandleAsync)
            .WithName("GetPlaceDetail")
            .WithTags("Geocoding")
            .RequireAuthorization("Customer");
    }

    private static async Task<IResult> HandleAsync(
        string placeId,
        IGeocodingService geocodingService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(placeId))
            return Results.BadRequest(new { error = "Place ID is required" });

        var detail = await geocodingService.GetPlaceDetailAsync(placeId, cancellationToken);

        if (detail is null)
            return Results.NotFound(new { error = "Place not found" });

        return Results.Ok(new
        {
            placeId = detail.PlaceId,
            formattedAddress = detail.FormattedAddress,
            latitude = detail.Latitude,
            longitude = detail.Longitude,
            locality = detail.Locality,
            pincode = detail.Pincode
        });
    }
}
