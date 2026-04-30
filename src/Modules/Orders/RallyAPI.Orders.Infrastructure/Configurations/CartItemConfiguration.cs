using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RallyAPI.Orders.Domain.Entities;

namespace RallyAPI.Orders.Infrastructure.Configurations;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.CartId)
            .HasColumnName("cart_id")
            .IsRequired();

        builder.Property(i => i.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired();

        builder.HasIndex(i => i.MenuItemId)
            .HasDatabaseName("ix_cart_items_menu_item_id");

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("INR")
            .IsRequired();

        builder.Property(i => i.Options)
            .HasColumnName("options")
            .HasColumnType("jsonb");

        builder.Property(i => i.SpecialInstructions)
            .HasColumnName("special_instructions")
            .HasMaxLength(500);

        builder.Property(i => i.AddedAt)
            .HasColumnName("added_at")
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(i => i.DomainEvents);
        builder.Ignore(i => i.DeletedAt);
    }
}
