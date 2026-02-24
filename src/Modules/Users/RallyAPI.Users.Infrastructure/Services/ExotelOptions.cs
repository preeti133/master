// File: src/Modules/Users/RallyAPI.Users.Infrastructure/Services/ExotelOptions.cs

namespace RallyAPI.Users.Infrastructure.Services;

public sealed class ExotelOptions
{
    public const string SectionName = "Exotel";

    /// <summary>
    /// Exotel Account SID
    /// </summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>
    /// Exotel API Key (HTTP Basic Auth username in URL)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Exotel API Token (HTTP Basic Auth password in URL)
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// Your ExoPhone number or approved transactional Sender ID.
    /// Must be linked to your ExoPhone and registered on DLT portal for India.
    /// e.g., "0XXXXXXXX" (ExoPhone) or "RALLYO" (sender ID linked to ExoPhone)
    /// </summary>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Exotel API subdomain based on your cluster region.
    /// Singapore: "api.exotel.com"
    /// Mumbai: "api.in.exotel.com"
    /// </summary>
    public string Subdomain { get; set; } = "api.in.exotel.com";

    /// <summary>
    /// DLT Entity ID — MANDATORY for Indian businesses sending SMS to Indian numbers.
    /// Registered on your operator's DLT portal (Jio, Airtel, VI, BSNL, etc.)
    /// </summary>
    public string DltEntityId { get; set; } = string.Empty;

    /// <summary>
    /// DLT Template ID for OTP messages — registered on your DLT portal.
    /// Optional but recommended to avoid template mismatch failures.
    /// </summary>
    public string DltTemplateId { get; set; } = string.Empty;
}
