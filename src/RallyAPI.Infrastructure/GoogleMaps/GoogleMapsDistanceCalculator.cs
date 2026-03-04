using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RallyAPI.Infrastructure.GoogleMaps.Models;
using RallyAPI.SharedKernel.Abstractions.Distance;

namespace RallyAPI.Infrastructure.GoogleMaps;

/// <summary>
/// Routes API — Compute Route Matrix implementation.
/// </summary>
public sealed class GoogleMapsDistanceCalculator : IDistanceCalculator
{
    private const string FieldMask =
        "originIndex,destinationIndex,distanceMeters,duration,status,condition";

    private readonly HttpClient _httpClient;
    private readonly GoogleMapsOptions _options;
    private readonly ILogger<GoogleMapsDistanceCalculator> _logger;

    public GoogleMapsDistanceCalculator(
        HttpClient httpClient,
        IOptions<GoogleMapsOptions> options,
        ILogger<GoogleMapsDistanceCalculator> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DistanceResult> GetDistanceAsync(
        double originLat,
        double originLng,
        double destinationLat,
        double destinationLng,
        CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("Google Maps API is disabled, using Haversine fallback");
            return GetHaversineFallback(originLat, originLng, destinationLat, destinationLng);
        }

        try
        {
            var request = BuildRequest(
                [RouteMatrixWaypointContainer.FromLatLng(originLat, originLng)],
                [RouteMatrixWaypointContainer.FromLatLng(destinationLat, destinationLng)]);

            _logger.LogDebug(
                "Requesting distance: ({OriginLat}, {OriginLng}) → ({DestLat}, {DestLng})",
                originLat, originLng, destinationLat, destinationLng);

            var elements = await SendRouteMatrixRequestAsync(request, ct);

            if (elements is null || elements.Count == 0)
            {
                _logger.LogWarning("Routes API returned empty response");
                return DistanceResult.Failure("Empty response from Routes API");
            }

            var element = elements[0];

            if (element.Status is { Code: not 0 })
            {
                _logger.LogWarning(
                    "Routes API error: Code={Code} - {Message}",
                    element.Status.Code, element.Status.Message);
                return DistanceResult.Failure(element.Status.Message ?? $"Error code {element.Status.Code}");
            }

            if (element.Condition != "ROUTE_EXISTS")
            {
                _logger.LogWarning("No route found: {Condition}", element.Condition);
                return DistanceResult.Failure(element.Condition ?? "No route found");
            }

            var durationSeconds = ParseDuration(element.Duration);

            _logger.LogInformation(
                "Distance calculated: {DistanceMeters}m, {DurationSeconds}s",
                element.DistanceMeters, durationSeconds);

            return DistanceResult.Success(
                distanceMeters: element.DistanceMeters,
                durationSeconds: durationSeconds,
                distanceText: FormatDistance(element.DistanceMeters),
                durationText: FormatDuration(durationSeconds));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Routes API");
            return GetHaversineFallback(originLat, originLng, destinationLat, destinationLng);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout calling Routes API");
            return GetHaversineFallback(originLat, originLng, destinationLat, destinationLng);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Routes API");
            return GetHaversineFallback(originLat, originLng, destinationLat, destinationLng);
        }
    }

    public async Task<IReadOnlyList<DistanceResult>> GetDistancesAsync(
        double originLat,
        double originLng,
        IReadOnlyList<(double lat, double lng)> destinations,
        CancellationToken ct = default)
    {
        if (!destinations.Any())
            return [];

        if (!_options.Enabled)
        {
            return destinations
                .Select(d => GetHaversineFallback(originLat, originLng, d.lat, d.lng))
                .ToList();
        }

        try
        {
            var request = BuildRequest(
                [RouteMatrixWaypointContainer.FromLatLng(originLat, originLng)],
                destinations.Select(d => RouteMatrixWaypointContainer.FromLatLng(d.lat, d.lng)).ToList());

            _logger.LogDebug(
                "Requesting distances to {Count} destinations",
                destinations.Count);

            var elements = await SendRouteMatrixRequestAsync(request, ct);

            if (elements is null || elements.Count == 0)
            {
                _logger.LogWarning("Routes API batch request failed, using fallback");
                return destinations
                    .Select(d => GetHaversineFallback(originLat, originLng, d.lat, d.lng))
                    .ToList();
            }

            // Index results by destinationIndex for correct ordering
            var elementsByDest = elements.ToDictionary(e => e.DestinationIndex);

            var results = new List<DistanceResult>();
            for (int i = 0; i < destinations.Count; i++)
            {
                if (elementsByDest.TryGetValue(i, out var el)
                    && el.Condition == "ROUTE_EXISTS"
                    && el.Status is null or { Code: 0 })
                {
                    var durationSeconds = ParseDuration(el.Duration);
                    results.Add(DistanceResult.Success(
                        distanceMeters: el.DistanceMeters,
                        durationSeconds: durationSeconds,
                        distanceText: FormatDistance(el.DistanceMeters),
                        durationText: FormatDuration(durationSeconds)));
                }
                else
                {
                    results.Add(GetHaversineFallback(
                        originLat, originLng,
                        destinations[i].lat, destinations[i].lng));
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch distance calculation");
            return destinations
                .Select(d => GetHaversineFallback(originLat, originLng, d.lat, d.lng))
                .ToList();
        }
    }

    // ── Helpers ─────────────────────────────────────────────

    private RouteMatrixRequest BuildRequest(
        List<RouteMatrixWaypointContainer> origins,
        List<RouteMatrixWaypointContainer> destinations) =>
        new()
        {
            Origins = origins,
            Destinations = destinations,
            TravelMode = _options.TravelMode
        };

    private async Task<List<RouteMatrixElement>?> SendRouteMatrixRequestAsync(
        RouteMatrixRequest body,
        CancellationToken ct)
    {
        var url = $"{_options.BaseUrl}/distanceMatrix:compute";

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };

        httpRequest.Headers.Add("X-Goog-Api-Key", _options.ApiKey);
        httpRequest.Headers.Add("X-Goog-FieldMask", FieldMask);

        var response = await _httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<RouteMatrixElement>>(
            cancellationToken: ct);
    }

    /// <summary>
    /// Parse protobuf Duration string (e.g. "567s") to integer seconds.
    /// </summary>
    private static int ParseDuration(string? duration)
    {
        if (string.IsNullOrEmpty(duration))
            return 0;

        var span = duration.AsSpan().TrimEnd('s');
        return int.TryParse(span, out var seconds) ? seconds : 0;
    }

    private static string FormatDistance(int meters)
    {
        var km = meters / 1000.0;
        return $"{km:F1} km";
    }

    private static string FormatDuration(int seconds)
    {
        var minutes = (int)Math.Ceiling(seconds / 60.0);
        return minutes < 60 ? $"{minutes} mins" : $"{minutes / 60}h {minutes % 60}m";
    }

    /// <summary>
    /// Fallback to Haversine with 1.4x multiplier for road estimate.
    /// </summary>
    private static DistanceResult GetHaversineFallback(
        double lat1, double lng1,
        double lat2, double lng2)
    {
        var straightLine = CalculateHaversine(lat1, lng1, lat2, lng2);
        var estimatedRoad = straightLine * 1.4; // Road distance estimate
        var estimatedMinutes = (int)Math.Ceiling(estimatedRoad / 20 * 60); // 20 km/h average

        return DistanceResult.Success(
            distanceMeters: (int)(estimatedRoad * 1000),
            durationSeconds: estimatedMinutes * 60,
            distanceText: $"{estimatedRoad:F1} km (est.)",
            durationText: $"{estimatedMinutes} mins (est.)");
    }

    private static double CalculateHaversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}