using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Owners.Queries.GetOutlets;

public sealed record GetOwnerOutletsQuery(Guid OwnerId) : IRequest<Result<IReadOnlyList<OutletSummaryResponse>>>;

public sealed record OutletSummaryResponse(
    Guid Id,
    string Name,
    string Email,
    string AddressLine,
    bool IsActive,
    bool IsAcceptingOrders,
    string? LogoUrl);
