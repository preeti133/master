using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RallyAPI.Orders.Infrastructure.Migrations
{
    public partial class AddPaymentWebhookFailed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "webhook_failed",
                schema: "orders",
                table: "payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "webhook_failed",
                schema: "orders",
                table: "payments");
        }
    }
}
