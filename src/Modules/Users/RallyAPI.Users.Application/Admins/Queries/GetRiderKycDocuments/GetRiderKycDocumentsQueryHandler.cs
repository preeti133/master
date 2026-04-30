using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Application.Admins.Queries.GetRiderKycDocuments;

internal sealed class GetRiderKycDocumentsQueryHandler
    : IRequestHandler<GetRiderKycDocumentsQuery, Result<RiderKycDocumentsResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly IRiderRepository _riderRepository;

    public GetRiderKycDocumentsQueryHandler(
        IAdminRepository adminRepository,
        IRiderRepository riderRepository)
    {
        _adminRepository = adminRepository;
        _riderRepository = riderRepository;
    }

    public async Task<Result<RiderKycDocumentsResponse>> Handle(
        GetRiderKycDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var admin = await _adminRepository.GetByIdAsync(request.RequestedByAdminId, cancellationToken);
        if (admin is null)
            return Result.Failure<RiderKycDocumentsResponse>(Error.NotFound("Admin", request.RequestedByAdminId));

        var rider = await _riderRepository.GetByIdWithKycAsync(request.RiderId, cancellationToken);
        if (rider is null)
            return Result.Failure<RiderKycDocumentsResponse>(Error.NotFound("Rider", request.RiderId));

        var documents = rider.KycDocuments
            .Select(d => new KycDocumentDto(
                d.Id,
                d.DocumentType.ToString(),
                d.PublicUrl,
                d.IsVerified,
                d.UploadedAt,
                d.VerifiedAt))
            .ToList();

        return Result.Success(new RiderKycDocumentsResponse(
            rider.Id,
            rider.Name,
            rider.KycStatus.ToString(),
            documents));
    }
}
