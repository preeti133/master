using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.SharedKernel.Storage;
using RallyAPI.Users.Application.Abstractions;
using RallyAPI.Users.Domain.Entities;

namespace RallyAPI.Users.Application.Riders.Commands.UploadKyc;

// ──────────────────────────────────────────────
// Command
// ──────────────────────────────────────────────

public sealed record GenerateRiderKycUploadUrlCommand(
    Guid RiderId,
    Guid RequestingRiderId,
    bool IsAdmin,
    RiderKycDocumentType DocumentType,
    string ContentType
) : IRequest<Result<GenerateRiderKycUploadUrlResponse>>;

public sealed record GenerateRiderKycUploadUrlResponse(
    string UploadUrl,
    string FileKey,
    DateTime ExpiresAt
);

// ──────────────────────────────────────────────
// Handler
// ──────────────────────────────────────────────

public sealed class GenerateRiderKycUploadUrlCommandHandler
    : IRequestHandler<GenerateRiderKycUploadUrlCommand, Result<GenerateRiderKycUploadUrlResponse>>
{
    private static readonly HashSet<string> AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    private static readonly Dictionary<string, string> ExtensionMap = new()
    {
        ["image/jpeg"] = "jpg",
        ["image/png"] = "png",
        ["image/webp"] = "webp",
    };

    private readonly IRiderRepository _riderRepository;
    private readonly IStorageService _storage;

    public GenerateRiderKycUploadUrlCommandHandler(
        IRiderRepository riderRepository,
        IStorageService storage)
    {
        _riderRepository = riderRepository;
        _storage = storage;
    }

    public async Task<Result<GenerateRiderKycUploadUrlResponse>> Handle(
        GenerateRiderKycUploadUrlCommand command,
        CancellationToken ct)
    {
        // 1. Validate content type
        var normalizedContentType = command.ContentType.ToLowerInvariant().Trim();
        if (!AllowedContentTypes.Contains(normalizedContentType))
            return Result.Failure<GenerateRiderKycUploadUrlResponse>(
                StorageErrors.InvalidFileType(command.ContentType));

        // 2. Ownership check
        if (!command.IsAdmin && command.RiderId != command.RequestingRiderId)
            return Result.Failure<GenerateRiderKycUploadUrlResponse>(
                Error.Forbidden("You can only upload your own KYC documents."));

        // 3. Verify rider exists
        var rider = await _riderRepository.GetByIdAsync(command.RiderId, ct);
        if (rider is null)
            return Result.Failure<GenerateRiderKycUploadUrlResponse>(
                Error.NotFound("Rider", command.RiderId));

        // 4. Build storage key
        // Format: riders/{riderId}/kyc/{documentType}/{timestamp}.{ext}
        var ext = ExtensionMap[normalizedContentType];
        var docTypeName = command.DocumentType.ToString().ToLowerInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var key = $"riders/{command.RiderId}/kyc/{docTypeName}/{timestamp}.{ext}";

        // 5. Generate presigned URL
        var result = await _storage.GenerateUploadUrlAsync(
            key,
            normalizedContentType,
            expiryMinutes: 10,
            ct);

        return Result.Success(new GenerateRiderKycUploadUrlResponse(
            result.UploadUrl,
            result.FileKey,
            result.ExpiresAt));
    }
}