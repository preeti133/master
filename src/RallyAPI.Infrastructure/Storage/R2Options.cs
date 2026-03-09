namespace RallyAPI.Infrastructure.Storage;

/// <summary>
/// Binds to the "R2" section in appsettings.json.
/// All values except PublicUrlBase come from Cloudflare R2 dashboard → API Tokens.
/// </summary>
public sealed class R2Options
{
    public const string SectionName = "R2";

    /// <summary>Your Cloudflare Account ID (found in R2 dashboard URL).</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>R2 API token Access Key ID.</summary>
    public string AccessKeyId { get; init; } = string.Empty;

    /// <summary>R2 API token Secret Access Key.</summary>
    public string SecretAccessKey { get; init; } = string.Empty;

    /// <summary>The R2 bucket name (e.g. "rally-media").</summary>
    public string BucketName { get; init; } = string.Empty;

    /// <summary>
    /// The public base URL for serving files (e.g. "https://media.yourdomain.com").
    /// Set this after enabling public access or connecting a custom domain in R2.
    /// </summary>
    public string PublicUrlBase { get; init; } = string.Empty;
}