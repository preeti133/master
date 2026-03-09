namespace RallyAPI.SharedKernel.Storage;

/// <summary>
/// Abstraction for cloud object storage (Cloudflare R2 / S3-compatible).
/// Lives in SharedKernel so any module can inject it without depending on infrastructure.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Generates a presigned PUT URL the client can use to upload a file directly to R2.
    /// The RallyAPI server never handles the file bytes.
    /// </summary>
    Task<PresignedUrlResult> GenerateUploadUrlAsync(
        string key,
        string contentType,
        int expiryMinutes = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Builds the public CDN URL for an already-uploaded file key.
    /// </summary>
    string BuildPublicUrl(string key);

    /// <summary>
    /// Deletes an object from storage. Used when replacing an existing image.
    /// </summary>
    Task<bool> DeleteAsync(string key, CancellationToken ct = default);
}