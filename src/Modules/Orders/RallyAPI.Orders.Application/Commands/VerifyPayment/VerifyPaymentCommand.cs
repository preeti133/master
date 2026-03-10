// File: src/Modules/Orders/RallyAPI.Orders.Application/Commands/VerifyPayment/VerifyPaymentCommand.cs

using MediatR;
using Microsoft.Extensions.Logging;
using RallyAPI.SharedKernel;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Commands.VerifyPayment;

public record VerifyPaymentCommand(
    string TxnId,
    Guid CustomerId
) : IRequest<Result<VerifyPaymentResponse>>;

public record VerifyPaymentResponse(
    string Status,
    string Amount,
    string? PayuId,
    string? PaymentMode
);

