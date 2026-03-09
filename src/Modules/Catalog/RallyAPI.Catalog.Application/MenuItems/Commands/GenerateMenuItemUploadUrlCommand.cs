using MediatR;
using RallyAPI.Catalog.Application.Abstractions;
using RallyAPI.SharedKernel.Results;
using RallyAPI.SharedKernel.Storage;

namespace RallyAPI.Catalog.Application.MenuItems.Commands.GenerateMenuItemUploadUrl;

// ──────────────────────────────────────────────
// Command
// ──────────────────────────────────────────────

/// <summary>
/// Step 1 of 2 for image upload.
/// Returns a presigned R2 PUT URL — the client uploads the image directly to R2.
/// After upload the client calls ConfirmMenuItemImageCommand (Step 2).
/// </summary>
public sealed record GenerateMenuItemUploadUrlCommand(
    Guid MenuItemId,
    /// <summary>The requesting restaurant's ID — used to verify ownership.</summary>
    Guid RequestingRestaurantId,
    /// <summary>Whether the requester is an admin (bypasses ownership check).</summary>
    bool IsAdmin,
    /// <summary>MIME type sent by the client: image/jpeg, image/png, or image/webp.</summary>
    string ContentType
) : IRequest<Result<GenerateMenuItemUploadUrlResponse>>;

public sealed record GenerateMenuItemUploadUrlResponse(
    string UploadUrl,
    string FileKey,
    DateTime ExpiresAt
);

// ──────────────────────────────────────────────
// Handler
// ──────────────────────────────────────────────

public sealed class GenerateMenuItemUploadUrlCommandHandler
    : IRequestHandler<GenerateMenuItemUploadUrlCommand, Result<GenerateMenuItemUploadUrlResponse>>
{
    // Allowed MIME types — enforced here AND the presigned URL is scoped to the ContentType,
    // so R2 will reject a PUT with a different Content-Type header.
    private static readonly HashSet<string> AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    private static readonly Dictionary<string, string> ExtensionMap = new()
    {
        ["image/jpeg"] = "jpg",
        ["image/png"] = "png",
        ["image/webp"] = "webp",
    };

    // 5 MB in bytes
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IStorageService _storage;

    public GenerateMenuItemUploadUrlCommandHandler(
        IMenuItemRepository menuItemRepository,
        IStorageService storage)
    {
        _menuItemRepository = menuItemRepository;
        _storage = storage;
    }

    public async Task<Result<GenerateMenuItemUploadUrlResponse>> Handle(
        GenerateMenuItemUploadUrlCommand command,
        CancellationToken ct)
    {
        // 1. Validate content type
        var normalizedContentType = command.ContentType.ToLowerInvariant().Trim();
        if (!AllowedContentTypes.Contains(normalizedContentType))
            return Result.Failure<GenerateMenuItemUploadUrlResponse>(
                StorageErrors.InvalidFileType(command.ContentType));

        // 2. Load the menu item
        var menuItem = await _menuItemRepository.GetByIdAsync(command.MenuItemId, ct);
        if (menuItem is null)
            return Result.Failure<GenerateMenuItemUploadUrlResponse>(
                Error.NotFound("Catelog",command.MenuItemId));

        // 3. Ownership check — restaurants can only upload for their own items
        if (!command.IsAdmin && menuItem.RestaurantId != command.RequestingRestaurantId)
            return Result.Failure<GenerateMenuItemUploadUrlResponse>(
                Error.Forbidden("You do not have permission to upload images for this menu item."));

        // 4. Build a deterministic, collision-resistant storage key
        //    Format: menu-items/{restaurantId}/{menuItemId}/{unixTimestamp}.{ext}
        var ext = ExtensionMap[normalizedContentType];
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var key = $"menu-items/{menuItem.RestaurantId}/{menuItem.Id}/{timestamp}.{ext}";

        // 5. Generate presigned URL (10 min expiry — enough for any reasonable upload)
        var result = await _storage.GenerateUploadUrlAsync(
            key,
            normalizedContentType,
            expiryMinutes: 10,
            ct);

        return Result.Success(new GenerateMenuItemUploadUrlResponse(
            result.UploadUrl,
            result.FileKey,
            result.ExpiresAt));
    }
}