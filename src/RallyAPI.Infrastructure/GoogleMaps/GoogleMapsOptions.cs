namespace RallyAPI.Infrastructure.GoogleMaps;

public sealed class GoogleMapsOptions
{
    public const string SectionName = "GoogleMaps";

    /// <summary>
    /// Google Maps API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Routes API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://routes.googleapis.com";

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Enable/disable the service.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Travel mode: DRIVE, BICYCLE, WALK, TWO_WHEELER, TRANSIT.
    /// </summary>
    public string TravelMode { get; set; } = "DRIVE";

    /// <summary>
    /// Region bias for results.
    /// </summary>
    public string Region { get; set; } = "in";
}