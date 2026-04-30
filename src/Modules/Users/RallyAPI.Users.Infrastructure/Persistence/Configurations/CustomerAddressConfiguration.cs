using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RallyAPI.Users.Domain.Entities;

namespace RallyAPI.Users.Infrastructure.Persistence.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        // Table mapping
        builder.ToTable("customer_addresses");

        // Primary Key
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Foreign Key
        builder.Property(a => a.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        // Address Value Object - Owned Entity (embedded as columns)
        builder.OwnsOne(a => a.Address, addressBuilder =>
        {
            addressBuilder.Property(addr => addr.AddressLine)
                .HasColumnName("address_line")
                .HasMaxLength(500)
                .IsRequired();

            addressBuilder.Property(addr => addr.Landmark)
                .HasColumnName("landmark")
                .HasMaxLength(255)
                .IsRequired(false);

            addressBuilder.Property(addr => addr.Latitude)
                .HasColumnName("latitude")
                .HasPrecision(10, 8)
                .IsRequired();

            addressBuilder.Property(addr => addr.Longitude)
                .HasColumnName("longitude")
                .HasPrecision(11, 8)
                .IsRequired();

            addressBuilder.Property(addr => addr.Label)
             .HasColumnName("label")
             .HasMaxLength(50)
             .IsRequired();

            addressBuilder.Property(addr => addr.PlaceId)
                .HasColumnName("place_id")
                .HasMaxLength(300)
                .IsRequired(false);
        });

        // IsDefault
        builder.Property(a => a.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false)
            .IsRequired();

        // Base Entity properties
        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(a => a.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired(false);

        // Index
        builder.HasIndex(a => a.CustomerId)
            .HasDatabaseName("idx_customer_addresses_customer");

        // Query Filter - Soft delete
        builder.HasQueryFilter(a => a.DeletedAt == null);

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);
    }
}