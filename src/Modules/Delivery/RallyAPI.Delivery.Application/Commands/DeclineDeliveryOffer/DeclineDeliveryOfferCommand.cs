using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Delivery.Application.Commands.DeclineDeliveryOffer;

public sealed record DeclineDeliveryOfferCommand : IRequest<Result>
{
    public required Guid OfferId { get; init; }
    public required Guid RiderId { get; init; }
    /// <summary>Optional reason the rider is declining (e.g. "too far", "busy").</summary>
    public string? Reason { get; init; }
}
