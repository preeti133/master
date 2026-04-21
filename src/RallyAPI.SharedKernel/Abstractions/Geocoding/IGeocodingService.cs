namespace RallyAPI.SharedKernel.Abstractions.Geocoding;

/// <summary>
/// Reverse geocoding and places autocomplete — wraps Google Maps APIs.
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Reverse geocode lat/lng to a formatted address.
    /// </summary>
    Task<ReverseGeocodeResult> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default);

    /// <summary>
    /// Autocomplete a partial address query. Returns up to <paramref name="maxResults"/> suggestions.
    /// </summary>
    Task<IReadOnlyList<PlaceSuggestion>> AutocompleteAsync(
        string input,
        double? sessionLatitude = null,
        double? sessionLongitude = null,
        int maxResults = 5,
        CancellationToken ct = default);

    /// <summary>
    /// Get full place details (lat/lng, formatted address) from a Place ID.
    /// </summary>
    Task<PlaceDetail?> GetPlaceDetailAsync(
        string placeId,
        CancellationToken ct = default);
}
