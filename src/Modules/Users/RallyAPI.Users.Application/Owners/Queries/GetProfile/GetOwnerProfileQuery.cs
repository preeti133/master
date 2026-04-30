using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Owners.Queries.GetProfile;

public sealed record GetOwnerProfileQuery(Guid OwnerId) : IRequest<Result<OwnerProfileResponse>>;

public sealed record OwnerProfileResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string? PanNumber,
    string? GstNumber,
    string? BankAccountNumber,
    string? BankIfscCode,
    string? BankAccountName,
    bool IsActive);
