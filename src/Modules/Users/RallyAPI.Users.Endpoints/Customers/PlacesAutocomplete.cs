using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.SharedKernel.Abstractions.Geocoding;

namespace RallyAPI.Users.Endpoints.Customers;

public class PlacesAutocomplete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/places/autocomplete", HandleAsync)
            .WithName("PlacesAutocomplete")
            .WithTags("Geocoding")
            .RequireAuthorization("Customer");
    }

    private static async Task<IResult> HandleAsync(
        string input,
        double? lat,
        double? lng,
        IGeocodingService geocodingService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            return Results.BadRequest(new { error = "Input must be at least 2 characters" });

        var suggestions = await geocodingService.AutocompleteAsync(
            input, lat, lng, maxResults: 5, ct: cancellationToken);

        return Results.Ok(new { suggestions });
    }
}
