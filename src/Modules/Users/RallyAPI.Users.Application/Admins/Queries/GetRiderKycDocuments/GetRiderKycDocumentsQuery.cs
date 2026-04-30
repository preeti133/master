using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Domain.Entities;

namespace RallyAPI.Users.Application.Admins.Queries.GetRiderKycDocuments;

public sealed record GetRiderKycDocumentsQuery(
    Guid RequestedByAdminId,
    Guid RiderId) : IRequest<Result<RiderKycDocumentsResponse>>;

public sealed record RiderKycDocumentsResponse(
    Guid RiderId,
    string RiderName,
    string KycStatus,
    List<KycDocumentDto> Documents);

public sealed record KycDocumentDto(
    Guid Id,
    string DocumentType,
    string PublicUrl,
    bool IsVerified,
    DateTime UploadedAt,
    DateTime? VerifiedAt);
