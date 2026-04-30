using MediatR;
using Microsoft.Extensions.Logging;
using RallyAPI.Delivery.Domain.Abstractions;
using RallyAPI.Delivery.Domain.Entities;
using RallyAPI.Delivery.Domain.Enums;
using RallyAPI.Delivery.Domain.Errors;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Delivery.Application.Commands.DeclineDeliveryOffer;

public sealed class DeclineDeliveryOfferCommandHandler : IRequestHandler<DeclineDeliveryOfferCommand, Result>
{
    private readonly IDeliveryRequestRepository _requestRepository;
    private readonly ILogger<DeclineDeliveryOfferCommandHandler> _logger;

    public DeclineDeliveryOfferCommandHandler(
        IDeliveryRequestRepository requestRepository,
        ILogger<DeclineDeliveryOfferCommandHandler> logger)
    {
        _requestRepository = requestRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeclineDeliveryOfferCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Rider {RiderId} declining offer {OfferId}. Reason: {Reason}",
            request.RiderId, request.OfferId, request.Reason ?? "none");

        // Find the delivery request containing this offer
        var deliveryRequests = await _requestRepository.GetByStatusAsync(
            DeliveryRequestStatus.SearchingOwnFleet, cancellationToken);

        DeliveryRequest? deliveryRequest = null;
        RiderOffer? offer = null;

        foreach (var dr in deliveryRequests)
        {
            var drWithOffers = await _requestRepository.GetByIdWithOffersAsync(dr.Id, cancellationToken);
            offer = drWithOffers?.RiderOffers.FirstOrDefault(o => o.Id == request.OfferId);
            if (offer is not null)
            {
                deliveryRequest = drWithOffers;
                break;
            }
        }

        if (deliveryRequest is null || offer is null)
            return Result.Failure(Error.Validation("Offer not found or delivery already assigned."));

        if (offer.RiderId != request.RiderId)
            return Result.Failure(Error.Validation("This offer was not sent to you."));

        if (offer.Status != RiderOfferStatus.Pending)
            return Result.Failure(DeliveryErrors.OfferAlreadyResponded);

        if (offer.IsExpired)
            return Result.Failure(DeliveryErrors.OfferExpired);

        offer.Reject(request.Reason);

        await _requestRepository.UpdateAsync(deliveryRequest, cancellationToken);

        _logger.LogInformation(
            "Offer {OfferId} declined by rider {RiderId}. Dispatch orchestrator will try next rider on timeout.",
            request.OfferId, request.RiderId);

        return Result.Success();
    }
}
