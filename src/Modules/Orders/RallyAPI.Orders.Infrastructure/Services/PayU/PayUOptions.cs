// File: src/Modules/Orders/RallyAPI.Orders.Infrastructure/Services/PayU/PayUOptions.cs

namespace RallyAPI.Orders.Infrastructure.Services.PayU;

public class PayUOptions
{
    public const string SectionName = "PayU";

    /// <summary>Merchant Key from PayU dashboard</summary>
    public string MerchantKey { get; set; } = string.Empty;

    /// <summary>Merchant Salt (v2) — NEVER expose to client. Store in env vars for production.</summary>
    public string MerchantSalt { get; set; } = string.Empty;

    /// <summary>Test: https://test.payu.in  |  Production: https://secure.payu.in</summary>
    public string BaseUrl { get; set; } = "https://test.payu.in";

    /// <summary>Where PayU redirects on successful payment</summary>
    public string SuccessUrl { get; set; } = string.Empty;

    /// <summary>Where PayU redirects on failed payment</summary>
    public string FailureUrl { get; set; } = string.Empty;
}