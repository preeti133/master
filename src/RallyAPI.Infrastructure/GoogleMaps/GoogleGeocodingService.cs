using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RallyAPI.SharedKernel.Abstractions.Geocoding;

namespace RallyAPI.Infrastructure.GoogleMaps;

/// <summary>
/// Google Maps Geocoding + Places (New) API implementation.
/// Proxies requests server-side so the API key never reaches the browser.
/// </summary>
public sealed class GoogleGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleMapsOptions _options;
    private readonly ILogger<GoogleGeocodingService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GoogleGeocodingService(
        HttpClient httpClient,
        IOptions<GoogleMapsOptions> options,
        ILogger<GoogleGeocodingService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ReverseGeocodeResult> ReverseGeocodeAsync(
        double latitude, double longitude, CancellationToken ct = default)
    {
        if (!_options.Enabled)
            return ReverseGeocodeResult.Failure("Google Maps API is disabled");

        try
        {
            var url = $"https://maps.googleapis.com/maps/api/geocode/json" +
                      $"?latlng={latitude},{longitude}" +
                      $"&key={_options.ApiKey}" +
                      $"&region={_options.Region}" +
                      $"&language=en";

            var response = await _httpClient.GetFromJsonAsync<GeocodeApiResponse>(url, JsonOptions, ct);

            if (response?.Status != "OK" || response.Results is not { Count: > 0 })
            {
                _logger.LogWarning("Reverse geocode failed: status={Status}", response?.Status);
                return ReverseGeocodeResult.Failure(response?.Status ?? "Empty response");
            }

            var best = response.Results[0];
            var locality = ExtractComponent(best, "locality");
            var pincode = ExtractComponent(best, "postal_code");

            return ReverseGeocodeResult.Success(
                best.FormattedAddress,
                best.PlaceId,
                locality,
                pincode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reverse geocode error for ({Lat}, {Lng})", latitude, longitude);
            return ReverseGeocodeResult.Failure("Geocoding service unavailable");
        }
    }

    public async Task<IReadOnlyList<PlaceSuggestion>> AutocompleteAsync(
        string input,
        double? sessionLatitude = null,
        double? sessionLongitude = null,
        int maxResults = 5,
        CancellationToken ct = default)
    {
        if (!_options.Enabled)
            return [];

        try
        {
            var url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json" +
                      $"?input={Uri.EscapeDataString(input)}" +
                      $"&key={_options.ApiKey}" +
                      $"&components=country:in" +
                      $"&language=en";

            if (sessionLatitude.HasValue && sessionLongitude.HasValue)
            {
                url += $"&location={sessionLatitude.Value},{sessionLongitude.Value}&radius=50000";
            }

            var response = await _httpClient.GetFromJsonAsync<AutocompleteApiResponse>(url, JsonOptions, ct);

            if (response?.Status != "OK" || response.Predictions is null)
            {
                _logger.LogWarning("Autocomplete failed: status={Status}", response?.Status);
                return [];
            }

            return response.Predictions
                .Take(maxResults)
                .Select(p => new PlaceSuggestion(
                    p.PlaceId,
                    p.Description,
                    p.StructuredFormatting?.MainText ?? p.Description,
                    p.StructuredFormatting?.SecondaryText ?? string.Empty))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Autocomplete error for input: {Input}", input);
            return [];
        }
    }

    public async Task<PlaceDetail?> GetPlaceDetailAsync(string placeId, CancellationToken ct = default)
    {
        if (!_options.Enabled)
            return null;

        try
        {
            var url = $"https://maps.googleapis.com/maps/api/place/details/json" +
                      $"?place_id={Uri.EscapeDataString(placeId)}" +
                      $"&key={_options.ApiKey}" +
                      $"&fields=place_id,formatted_address,geometry,address_components" +
                      $"&language=en";

            var response = await _httpClient.GetFromJsonAsync<PlaceDetailApiResponse>(url, JsonOptions, ct);

            if (response?.Status != "OK" || response.Result is null)
            {
                _logger.LogWarning("Place detail failed: status={Status}", response?.Status);
                return null;
            }

            var r = response.Result;
            var locality = ExtractComponent(r, "locality");
            var pincode = ExtractComponent(r, "postal_code");

            return new PlaceDetail(
                r.PlaceId,
                r.FormattedAddress,
                r.Geometry.Location.Lat,
                r.Geometry.Location.Lng,
                locality,
                pincode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Place detail error for placeId: {PlaceId}", placeId);
            return null;
        }
    }

    // ── Helpers ─────────────────────────────────────────────

    private static string? ExtractComponent(GeocodeResult result, string type)
    {
        return result.AddressComponents?
            .FirstOrDefault(c => c.Types.Contains(type))?
            .LongName;
    }

    private static string? ExtractComponent(PlaceDetailResult result, string type)
    {
        return result.AddressComponents?
            .FirstOrDefault(c => c.Types.Contains(type))?
            .LongName;
    }

    // ── Google API response DTOs ────────────────────────────

    private sealed class GeocodeApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("results")]
        public List<GeocodeResult> Results { get; set; } = new();
    }

    private sealed class GeocodeResult
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = string.Empty;

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; } = string.Empty;

        [JsonPropertyName("address_components")]
        public List<AddressComponent>? AddressComponents { get; set; }
    }

    private sealed class AutocompleteApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("predictions")]
        public List<AutocompletePrediction>? Predictions { get; set; }
    }

    private sealed class AutocompletePrediction
    {
        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("structured_formatting")]
        public StructuredFormatting? StructuredFormatting { get; set; }
    }

    private sealed class StructuredFormatting
    {
        [JsonPropertyName("main_text")]
        public string MainText { get; set; } = string.Empty;

        [JsonPropertyName("secondary_text")]
        public string SecondaryText { get; set; } = string.Empty;
    }

    private sealed class PlaceDetailApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("result")]
        public PlaceDetailResult? Result { get; set; }
    }

    private sealed class PlaceDetailResult
    {
        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; } = string.Empty;

        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = string.Empty;

        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; } = new();

        [JsonPropertyName("address_components")]
        public List<AddressComponent>? AddressComponents { get; set; }
    }

    private sealed class Geometry
    {
        [JsonPropertyName("location")]
        public LatLng Location { get; set; } = new();
    }

    private sealed class LatLng
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    private sealed class AddressComponent
    {
        [JsonPropertyName("long_name")]
        public string LongName { get; set; } = string.Empty;

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = new();
    }
}
