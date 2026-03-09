using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.SharedKernel.Storage;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Application.Restaurants.Commands.UploadLogo;

// ──────────────────────────────────────────────
// Command
// ──────────────────────────────────────────────

public sealed record GenerateRestaurantLogoUploadUrlCommand(
    Guid RestaurantId,
    Guid RequestingRestaurantId,
    bool IsAdmin,
    string ContentType
) : IRequest<Result<GenerateRestaurantLogoUploadUrlResponse>>;

public sealed record GenerateRestaurantLogoUploadUrlResponse(
    string UploadUrl,
    string FileKey,
    DateTime ExpiresAt
);

// ──────────────────────────────────────────────
// Handler
// ──────────────────────────────────────────────

public sealed class GenerateRestaurantLogoUploadUrlCommandHandler
    : IRequestHandler<GenerateRestaurantLogoUploadUrlCommand, Result<GenerateRestaurantLogoUploadUrlResponse>>
{
    private static readonly HashSet<string> AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    private static readonly Dictionary<string, string> ExtensionMap = new()
    {
        ["image/jpeg"] = "jpg",
        ["image/png"] = "png",
        ["image/webp"] = "webp",
    };

    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IStorageService _storage;

    public GenerateRestaurantLogoUploadUrlCommandHandler(
        IRestaurantRepository restaurantRepository,
        IStorageService storage)
    {
        _restaurantRepository = restaurantRepository;
        _storage = storage;
    }

    public async Task<Result<GenerateRestaurantLogoUploadUrlResponse>> Handle(
        GenerateRestaurantLogoUploadUrlCommand command,
        CancellationToken ct)
    {
        // 1. Validate content type
        var normalizedContentType = command.ContentType.ToLowerInvariant().Trim();
        if (!AllowedContentTypes.Contains(normalizedContentType))
            return Result.Failure<GenerateRestaurantLogoUploadUrlResponse>(
                StorageErrors.InvalidFileType(command.ContentType));

        // 2. Ownership check — restaurants can only upload their own logo
        if (!command.IsAdmin && command.RestaurantId != command.RequestingRestaurantId)
            return Result.Failure<GenerateRestaurantLogoUploadUrlResponse>(
                Error.Forbidden("You can only update your own restaurant logo."));

        // 3. Verify restaurant exists
        var restaurant = await _restaurantRepository.GetByIdAsync(command.RestaurantId, ct);
        if (restaurant is null)
            return Result.Failure<GenerateRestaurantLogoUploadUrlResponse>(
                Error.NotFound("Restaurant", command.RestaurantId));

        // 4. Build storage key
        // Format: restaurants/{restaurantId}/logo/{timestamp}.{ext}
        var ext = ExtensionMap[normalizedContentType];
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var key = $"restaurants/{command.RestaurantId}/logo/{timestamp}.{ext}";

        // 5. Generate presigned URL (10 min expiry)
        var result = await _storage.GenerateUploadUrlAsync(
            key,
            normalizedContentType,
            expiryMinutes: 10,
            ct);

        return Result.Success(new GenerateRestaurantLogoUploadUrlResponse(
            result.UploadUrl,
            result.FileKey,
            result.ExpiresAt));
    }
}