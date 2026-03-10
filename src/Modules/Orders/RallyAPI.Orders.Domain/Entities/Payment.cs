// File: src/Modules/Orders/RallyAPI.Orders.Domain/Entities/Payment.cs

using RallyAPI.Orders.Domain.Enums;
using RallyAPI.SharedKernel.Domain;

namespace RallyAPI.Orders.Domain.Entities;

public class Payment : BaseEntity
{
    // === Core Fields ===
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }

    /// <summary>Your unique transaction ID sent to PayU (format: RALLY-{orderNumber})</summary>
    public string TxnId { get; private set; } = string.Empty;

    /// <summary>PayU's internal transaction ID (mihpayid) — set on callback</summary>
    public string? PayuId { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "INR";

    // === Status ===
    public PaymentStatus Status { get; private set; }

    /// <summary>Raw status string from PayU (success/failure/pending)</summary>
    public string? PayuStatus { get; private set; }

    /// <summary>Payment mode from PayU: CC, DC, NB, UPI, WALLET, EMI</summary>
    public string? PaymentMode { get; private set; }

    /// <summary>Bank reference number from PayU response</summary>
    public string? BankRefNum { get; private set; }

    /// <summary>Error message if payment failed</summary>
    public string? ErrorMessage { get; private set; }

    // === Customer Info (sent to PayU for hash) ===
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;

    // === Refund Fields ===
    public string? RefundRequestId { get; private set; }
    public decimal? RefundAmount { get; private set; }
    public string? RefundStatus { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    // EF Core
    private Payment() { }

    // === Factory ===
    public static Payment Create(
        Guid orderId,
        Guid customerId,
        string txnId,
        decimal amount,
        string customerName,
        string customerEmail,
        string customerPhone)
    {
        if (orderId == Guid.Empty) throw new ArgumentException("Order ID is required", nameof(orderId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required", nameof(customerId));
        if (string.IsNullOrWhiteSpace(txnId)) throw new ArgumentException("Transaction ID is required", nameof(txnId));
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            TxnId = txnId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CustomerName = customerName ?? string.Empty,
            CustomerEmail = customerEmail ?? string.Empty,
            CustomerPhone = customerPhone ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return payment;
    }

    // === Status Transitions ===

    public void MarkInitiated()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot initiate payment in {Status} status");

        Status = PaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSuccess(string payuId, string paymentMode, string? bankRefNum)
    {
        if (Status == PaymentStatus.Paid)
            return; // Idempotent — already processed

        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark payment as success in {Status} status");

        PayuId = payuId;
        PayuStatus = "success";
        PaymentMode = paymentMode;
        BankRefNum = bankRefNum;
        Status = PaymentStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string? payuId, string? errorMessage)
    {
        if (Status == PaymentStatus.Failed)
            return; // Idempotent

        PayuId = payuId;
        PayuStatus = "failure";
        ErrorMessage = errorMessage;
        Status = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRefundInitiated(string refundRequestId, decimal refundAmount)
    {
        if (!Status.IsRefundable())
            throw new InvalidOperationException($"Cannot refund payment in {Status} status");

        RefundRequestId = refundRequestId;
        RefundAmount = refundAmount;
        RefundStatus = "Queued";
        Status = refundAmount >= Amount ? PaymentStatus.RefundInitiated : PaymentStatus.RefundInitiated;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRefundCompleted()
    {
        if (Status != PaymentStatus.RefundInitiated)
            throw new InvalidOperationException($"Cannot complete refund in {Status} status");

        RefundStatus = "Success";
        RefundedAt = DateTime.UtcNow;
        Status = (RefundAmount.HasValue && RefundAmount.Value < Amount)
            ? PaymentStatus.PartiallyRefunded
            : PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }
}