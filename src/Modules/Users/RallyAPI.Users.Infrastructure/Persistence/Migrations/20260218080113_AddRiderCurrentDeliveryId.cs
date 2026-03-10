using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RallyAPI.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRiderCurrentDeliveryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "current_delivery_id",
                table: "riders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "current_delivery_assigned_at",
                table: "riders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_riders_availability",
                table: "riders",
                columns: new[] { "is_online", "is_active", "current_delivery_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_riders_availability;");
            migrationBuilder.Sql("ALTER TABLE riders DROP COLUMN IF EXISTS current_delivery_id;");
            migrationBuilder.Sql("ALTER TABLE riders DROP COLUMN IF EXISTS current_delivery_assigned_at;");
        }
    }
}
