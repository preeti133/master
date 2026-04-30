using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RallyAPI.Orders.Domain.Entities;

namespace RallyAPI.Orders.Infrastructure.Configurations;

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.HasIndex(c => c.CustomerId)
            .IsUnique()
            .HasDatabaseName("ix_carts_customer_id");

        builder.Property(c => c.RestaurantId)
            .HasColumnName("restaurant_id")
            .IsRequired();

        builder.Property(c => c.RestaurantName)
            .HasColumnName("restaurant_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // CartItem relationship
        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(c => c.SubTotal);
        builder.Ignore(c => c.ItemCount);
        builder.Ignore(c => c.DomainEvents);
        builder.Ignore(c => c.DeletedAt);
    }
}
