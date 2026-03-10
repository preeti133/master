using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RallyAPI.Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payments",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    txn_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payu_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "INR"),
                    status = table.Column<int>(type: "integer", nullable: false),
                    payu_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    payment_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bank_ref_num = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    customer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    customer_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    refund_request_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    refund_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    refund_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    refunded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payments_customer_id",
                schema: "orders",
                table: "payments",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_order_id",
                schema: "orders",
                table: "payments",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_txn_id",
                schema: "orders",
                table: "payments",
                column: "txn_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payments",
                schema: "orders");
        }
    }
}
