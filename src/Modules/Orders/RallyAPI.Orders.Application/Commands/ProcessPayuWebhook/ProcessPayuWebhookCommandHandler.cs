// File: src/Modules/Orders/RallyAPI.Orders.Application/Commands/ProcessPayuWebhook/ProcessPayuWebhookCommandHandler.cs

using MediatR;
using Microsoft.Extensions.Logging;
using RallyAPI.Orders.Domain.Repositories;
using RallyAPI.SharedKernel;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Orders.Application.Abstractions;

namespace RallyAPI.Orders.Application.Commands.ProcessPayuWebhook;

public class ProcessPayuWebhookCommandHandler
    : IRequestHandler<ProcessPayuWebhookCommand, Result<bool>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPayUService _payUService;
    private readonly ILogger<ProcessPayuWebhookCommandHandler> _logger;

    public ProcessPayuWebhookCommandHandler(
        IPaymentRepository paymentRepository,
        IPayUService payUService,
        ILogger<ProcessPayuWebhookCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _payUService = payUService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ProcessPayuWebhookCommand command, CancellationToken ct)
    {
        var formData = command.FormData;

        var txnId = formData.GetValueOrDefault("txnid", "");
        var status = formData.GetValueOrDefault("status", "");
        var payuId = formData.GetValueOrDefault("mihpayid", "");
        var mode = formData.GetValueOrDefault("mode", "");
        var bankRefNum = formData.GetValueOrDefault("bank_ref_num", "");
        var errorMessage = formData.GetValueOrDefault("error_Message", "");

        _logger.LogInformation(
            "PayU webhook received — TxnId: {TxnId}, Status: {Status}, PayuId: {PayuId}, Mode: {Mode}",
            txnId, status, payuId, mode);

        // 1. Verify reverse hash
        if (!_payUService.VerifyWebhookHash(formData))
        {
            _logger.LogWarning("PayU webhook hash verification FAILED for TxnId {TxnId}", txnId);
            return Result.Failure<bool>(
                Error.Create("Payment.InvalidHash", "Webhook hash verification failed"));
        }

        // 2. Find payment by txnId
        var payment = await _paymentRepository.GetByTxnIdAsync(txnId, ct);
        if (payment is null)
        {
            _logger.LogWarning("PayU webhook: no payment found for TxnId {TxnId}", txnId);
            return Result.Failure<bool>(
                Error.Create("Payment.NotFound", $"No payment found for transaction {txnId}"));
        }

        // 3. Idempotency check — if already processed, skip
        if (payment.Status == Domain.Enums.PaymentStatus.Paid)
        {
            _logger.LogInformation("PayU webhook: payment already marked as Paid for TxnId {TxnId}, skipping", txnId);
            return Result.Success(true);
        }

        if (payment.Status == Domain.Enums.PaymentStatus.Failed && status != "success")
        {
            _logger.LogInformation("PayU webhook: payment already marked as Failed for TxnId {TxnId}, skipping", txnId);
            return Result.Success(true);
        }

        // 4. Update payment based on PayU status
        if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
        {
            payment.MarkSuccess(payuId, mode, bankRefNum);
            await _paymentRepository.UpdateAsync(payment, ct);

            _logger.LogInformation(
                "Payment SUCCESS for Order {OrderId}, TxnId: {TxnId}, PayuId: {PayuId}, Mode: {Mode}",
                payment.OrderId, txnId, payuId, mode);

            // TODO: If using payment-first flow where Order doesn't exist yet,
            // trigger PlaceOrder here via MediatR.
            // For now, the Order already exists (created before payment initiation).
            // Update the Order's PaymentStatus:
            // await _mediator.Send(new UpdateOrderPaymentStatusCommand(payment.OrderId, payuId, txnId));
        }
        else
        {
            payment.MarkFailed(payuId, errorMessage);
            await _paymentRepository.UpdateAsync(payment, ct);

            _logger.LogWarning(
                "Payment FAILED for Order {OrderId}, TxnId: {TxnId}, Error: {Error}",
                payment.OrderId, txnId, errorMessage);
        }

        return Result.Success(true);
    }
}