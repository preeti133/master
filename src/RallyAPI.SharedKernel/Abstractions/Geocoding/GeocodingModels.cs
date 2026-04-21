namespace RallyAPI.SharedKernel.Abstractions.Geocoding;

/// <summary>
/// Result of a reverse geocode lookup.
/// </summary>
public sealed record ReverseGeocodeResult
{
    public bool IsSuccess { get; init; }
    public string? FormattedAddress { get; init; }
    public string? PlaceId { get; init; }
    public string? Locality { get; init; }
    public string? Pincode { get; init; }
    public string? Error { get; init; }

    public static ReverseGeocodeResult Success(
        string formattedAddress, string placeId, string? locality, string? pincode)
        => new()
        {
            IsSuccess = true,
            FormattedAddress = formattedAddress,
            PlaceId = placeId,
            Locality = locality,
            Pincode = pincode
        };

    public static ReverseGeocodeResult Failure(string error)
        => new() { IsSuccess = false, Error = error };
}

/// <summary>
/// A single autocomplete suggestion from Google Places.
/// </summary>
public sealed record PlaceSuggestion(
    string PlaceId,
    string Description,
    string MainText,
    string SecondaryText);

/// <summary>
/// Full details for a resolved Place ID.
/// </summary>
public sealed record PlaceDetail(
    string PlaceId,
    string FormattedAddress,
    double Latitude,
    double Longitude,
    string? Locality,
    string? Pincode);
