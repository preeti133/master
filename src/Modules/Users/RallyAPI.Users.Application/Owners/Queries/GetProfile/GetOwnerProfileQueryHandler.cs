using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Application.Owners.Queries.GetProfile;

internal sealed class GetOwnerProfileQueryHandler
    : IRequestHandler<GetOwnerProfileQuery, Result<OwnerProfileResponse>>
{
    private readonly IRestaurantOwnerRepository _ownerRepository;

    public GetOwnerProfileQueryHandler(IRestaurantOwnerRepository ownerRepository)
    {
        _ownerRepository = ownerRepository;
    }

    public async Task<Result<OwnerProfileResponse>> Handle(
        GetOwnerProfileQuery request,
        CancellationToken cancellationToken)
    {
        var owner = await _ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);
        if (owner is null)
            return Result.Failure<OwnerProfileResponse>(Error.NotFound("RestaurantOwner", request.OwnerId));

        return new OwnerProfileResponse(
            owner.Id,
            owner.Name,
            owner.Email.Value,
            owner.Phone.Value,
            owner.PanNumber,
            owner.GstNumber,
            Mask(owner.BankAccountNumber),
            owner.BankIfscCode,
            owner.BankAccountName,
            owner.IsActive);
    }

    private static string? Mask(string? accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
            return accountNumber;
        return new string('*', accountNumber.Length - 4) + accountNumber[^4..];
    }
}
