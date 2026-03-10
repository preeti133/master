// File: src/Modules/Orders/RallyAPI.Orders.Application/Abstractions/IPayUService.cs

namespace RallyAPI.Orders.Application.Abstractions;

public interface IPayUService
{
    PayUCheckoutParams GenerateCheckoutParams(
        string txnId, decimal amount, string productInfo,
        string firstName, string email, string phone);
    bool VerifyWebhookHash(Dictionary<string, string> formData);
    Task<PayUVerifyResult?> VerifyPaymentAsync(string txnId);
    Task<PayURefundResult?> RefundTransactionAsync(string payuId, string uniqueToken, decimal amount);
}

public class PayUCheckoutParams
{
    public string Key { get; set; } = string.Empty;
    public string TxnId { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string ProductInfo { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Surl { get; set; } = string.Empty;
    public string Furl { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string PayUBaseUrl { get; set; } = string.Empty;
}

public class PayUVerifyResult
{
    public string Status { get; set; } = string.Empty;
    public string PayuId { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string? Mode { get; set; }
    public string? BankRefNum { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PayURefundResult
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RequestId { get; set; }
}