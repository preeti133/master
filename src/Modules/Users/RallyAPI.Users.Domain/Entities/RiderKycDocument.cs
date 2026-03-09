// ============================================================
// FILE 1: RiderKycDocument.cs
// Location: src/Modules/Users/RallyAPI.Users.Domain/Entities/RiderKycDocument.cs
// ============================================================

using MediatR;
using RallyAPI.SharedKernel.Domain;
using RallyAPI.SharedKernel.Results;
using RallyAPI.SharedKernel.Storage;

namespace RallyAPI.Users.Domain.Entities;

/// <summary>
/// Represents a KYC document uploaded by a rider.
/// Each rider can have multiple documents (Aadhaar front/back, driving licence, vehicle RC).
/// Admin reviews these and flips IsVerified = true to complete KYC.
/// </summary>
public sealed class RiderKycDocument : BaseEntity
{
    private RiderKycDocument() { } // EF Core

    public Guid RiderId { get; private set; }
    public RiderKycDocumentType DocumentType { get; private set; }

    /// <summary>R2 storage key (not the public URL) — used for deletes/replacements.</summary>
    public string FileKey { get; private set; } = string.Empty;

    /// <summary>Public CDN URL saved after upload confirmation.</summary>
    public string PublicUrl { get; private set; } = string.Empty;

    public DateTime UploadedAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public bool IsVerified { get; private set; }

    public static RiderKycDocument Create(
        Guid riderId,
        RiderKycDocumentType documentType,
        string fileKey,
        string publicUrl)
    {
        return new RiderKycDocument
        {
            Id = Guid.NewGuid(),
            RiderId = riderId,
            DocumentType = documentType,
            FileKey = fileKey,
            PublicUrl = publicUrl,
            UploadedAt = DateTime.UtcNow,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>Admin verifies a document after manual review.</summary>
    public void Verify()
    {
        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Update file reference when rider re-uploads a document.</summary>
    public void UpdateFile(string fileKey, string publicUrl)
    {
        FileKey = fileKey;
        PublicUrl = publicUrl;
        IsVerified = false; // Re-upload resets verification — admin must re-review
        VerifiedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum RiderKycDocumentType
{
    AadhaarFront = 1,
    AadhaarBack = 2,
    DrivingLicense = 3,
    VehicleRC = 4,
}

