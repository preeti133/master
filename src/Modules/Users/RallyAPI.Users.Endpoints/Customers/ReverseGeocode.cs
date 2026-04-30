using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.SharedKernel.Abstractions.Geocoding;

namespace RallyAPI.Users.Endpoints.Customers;

public class ReverseGeocode : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/geocode/reverse", HandleAsync)
            .WithName("ReverseGeocode")
            .WithTags("Geocoding")
            .RequireAuthorization("Customer");
    }

    private static async Task<IResult> HandleAsync(
        double lat,
        double lng,
        IGeocodingService geocodingService,
        CancellationToken cancellationToken)
    {
        if (lat < 6 || lat > 38 || lng < 68 || lng > 98)
            return Results.BadRequest(new { error = "Coordinates outside India bounds" });

        var result = await geocodingService.ReverseGeocodeAsync(lat, lng, cancellationToken);

        if (!result.IsSuccess)
            return Results.BadRequest(new { error = result.Error });

        return Results.Ok(new
        {
            formattedAddress = result.FormattedAddress,
            placeId = result.PlaceId,
            locality = result.Locality,
            pincode = result.Pincode
        });
    }
}
