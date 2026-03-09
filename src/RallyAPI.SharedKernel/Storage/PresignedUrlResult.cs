namespace RallyAPI.SharedKernel.Storage;

/// <summary>
/// Returned to the client after generating a presigned upload URL.
/// The client uses UploadUrl to PUT the file directly to R2,
/// then sends FileKey back to the confirm endpoint.
/// </summary>
public sealed record PresignedUrlResult(
    /// <summary>The presigned PUT URL — valid for ExpiresAt.</summary>
    string UploadUrl,

    /// <summary>
    /// The storage key for this file (e.g. "menu-items/{restaurantId}/{itemId}/1234567890.jpg").
    /// The client must send this back in the confirm request.
    /// </summary>
    string FileKey,

    /// <summary>When the presigned URL expires.</summary>
    DateTime ExpiresAt
);