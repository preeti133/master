using System.Text.Json.Serialization;

namespace RallyAPI.Infrastructure.GoogleMaps.Models;

// ── Request ────────────────────────────────────────────────

public sealed class RouteMatrixRequest
{
    [JsonPropertyName("origins")]
    public List<RouteMatrixWaypointContainer> Origins { get; set; } = [];

    [JsonPropertyName("destinations")]
    public List<RouteMatrixWaypointContainer> Destinations { get; set; } = [];

    [JsonPropertyName("travelMode")]
    public string TravelMode { get; set; } = "DRIVE";
}

public sealed class RouteMatrixWaypointContainer
{
    [JsonPropertyName("waypoint")]
    public RouteMatrixWaypoint Waypoint { get; set; } = new();

    public static RouteMatrixWaypointContainer FromLatLng(double lat, double lng) =>
        new()
        {
            Waypoint = new RouteMatrixWaypoint
            {
                Location = new RouteMatrixLocation
                {
                    LatLng = new RouteMatrixLatLng
                    {
                        Latitude = lat,
                        Longitude = lng
                    }
                }
            }
        };
}

public sealed class RouteMatrixWaypoint
{
    [JsonPropertyName("location")]
    public RouteMatrixLocation Location { get; set; } = new();
}

public sealed class RouteMatrixLocation
{
    [JsonPropertyName("latLng")]
    public RouteMatrixLatLng LatLng { get; set; } = new();
}

public sealed class RouteMatrixLatLng
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}

// ── Response ───────────────────────────────────────────────

public sealed class RouteMatrixElement
{
    [JsonPropertyName("originIndex")]
    public int OriginIndex { get; set; }

    [JsonPropertyName("destinationIndex")]
    public int DestinationIndex { get; set; }

    [JsonPropertyName("distanceMeters")]
    public int DistanceMeters { get; set; }

    /// <summary>
    /// Protobuf Duration format, e.g. "567s".
    /// </summary>
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("status")]
    public RouteMatrixStatus? Status { get; set; }

    [JsonPropertyName("condition")]
    public string? Condition { get; set; }
}

public sealed class RouteMatrixStatus
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
