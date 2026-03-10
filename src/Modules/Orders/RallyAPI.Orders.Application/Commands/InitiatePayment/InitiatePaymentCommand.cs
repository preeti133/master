// File: src/Modules/Orders/RallyAPI.Orders.Application/Commands/InitiatePayment/InitiatePaymentCommand.cs

using MediatR;
using RallyAPI.SharedKernel;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Commands.InitiatePayment;

public record InitiatePaymentCommand(
    Guid OrderId,
    Guid CustomerId
) : IRequest<Result<InitiatePaymentResponse>>;

public record InitiatePaymentResponse(
    string Key,
    string TxnId,
    string Amount,
    string ProductInfo,
    string FirstName,
    string Email,
    string Phone,
    string Surl,
    string Furl,
    string Hash,
    string PayUBaseUrl
);