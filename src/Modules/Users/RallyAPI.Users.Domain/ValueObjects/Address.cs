using RallyAPI.SharedKernel.Domain;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string AddressLine { get; }
    public string? Landmark { get; }
    public decimal Latitude { get; }
    public decimal Longitude { get; }
    public string Label { get; } // Home, Work, Other
    public string? PlaceId { get; } // Google Places ID for re-fetching details

    private Address(
        string addressLine,
        string? landmark,
        decimal latitude,
        decimal longitude,
        string label,
        string? placeId)
    {
        AddressLine = addressLine;
        Landmark = landmark;
        Latitude = latitude;
        Longitude = longitude;
        Label = label;
        PlaceId = placeId;
    }

    public static Result<Address> Create(
        string? addressLine,
        string? landmark,
        decimal latitude,
        decimal longitude,
        string? label,
        string? placeId = null)
    {
        if (string.IsNullOrWhiteSpace(addressLine))
            return Result.Failure<Address>(Error.Validation("Address line is required."));

        if (addressLine.Length > 500)
            return Result.Failure<Address>(Error.Validation("Address line is too long."));

        // Basic lat/lng validation (India bounds approximately)
        if (latitude < 6 || latitude > 38)
            return Result.Failure<Address>(Error.Validation("Invalid latitude for India."));

        if (longitude < 68 || longitude > 98)
            return Result.Failure<Address>(Error.Validation("Invalid longitude for India."));

        var validLabels = new[] { "Home", "Work", "Other" };
        var normalizedLabel = label?.Trim() ?? "Other";

        if (!validLabels.Contains(normalizedLabel, StringComparer.OrdinalIgnoreCase))
            normalizedLabel = "Other";

        return new Address(
            addressLine.Trim(),
            landmark?.Trim(),
            latitude,
            longitude,
            normalizedLabel,
            placeId?.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AddressLine;
        yield return Landmark ?? string.Empty;
        yield return Latitude;
        yield return Longitude;
        yield return Label;
        yield return PlaceId ?? string.Empty;
    }
}