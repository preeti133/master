using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RallyAPI.Users.Domain.Entities;

namespace RallyAPI.Users.Infrastructure.Persistence.Configurations;

public class RiderKycDocumentConfiguration : IEntityTypeConfiguration<RiderKycDocument>
{
    public void Configure(EntityTypeBuilder<RiderKycDocument> builder)
    {
        builder.ToTable("rider_kyc_documents", "users");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.RiderId).HasColumnName("rider_id");

        builder.Property(e => e.DocumentType)
               .HasColumnName("document_type")
               .HasMaxLength(50)
               .HasConversion<string>();

        builder.Property(e => e.FileKey)
               .HasColumnName("file_key")
               .HasMaxLength(500);

        builder.Property(e => e.PublicUrl)
               .HasColumnName("public_url")
               .HasMaxLength(500);

        builder.Property(e => e.UploadedAt).HasColumnName("uploaded_at");
        builder.Property(e => e.VerifiedAt).HasColumnName("verified_at");
        builder.Property(e => e.IsVerified)
               .HasColumnName("is_verified")
               .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.RiderId)
               .HasDatabaseName("idx_rider_kyc_documents_rider_id");

        // One document per type per rider
        builder.HasIndex(e => new { e.RiderId, e.DocumentType })
               .IsUnique()
               .HasDatabaseName("idx_rider_kyc_documents_rider_type");
    }
}