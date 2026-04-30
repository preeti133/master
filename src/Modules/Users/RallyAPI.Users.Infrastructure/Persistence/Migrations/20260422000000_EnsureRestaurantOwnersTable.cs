using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RallyAPI.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnsureRestaurantOwnersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent recovery migration. An earlier migration
            // (20260331100000_AddRestaurantOwnerAndOutletSupport) was authored without a
            // Designer.cs and never picked up by EF, leaving the users.restaurant_owners
            // table missing on some environments even though the model snapshot already
            // declares the entity. This migration brings stragglers up to date and is a
            // no-op where the table already exists.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS users.restaurant_owners (
                    id                   uuid PRIMARY KEY,
                    name                 varchar(255) NOT NULL,
                    email                varchar(255) NOT NULL,
                    password_hash        varchar(255) NOT NULL,
                    phone                varchar(15)  NOT NULL,
                    pan_number           varchar(10),
                    gst_number           varchar(15),
                    bank_account_number  varchar(20),
                    bank_ifsc_code       varchar(11),
                    bank_account_name    varchar(255),
                    is_active            boolean NOT NULL DEFAULT true,
                    created_at           timestamptz NOT NULL,
                    updated_at           timestamptz NOT NULL,
                    deleted_at           timestamptz,
                    version              integer NOT NULL DEFAULT 1
                );
            ");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS idx_restaurant_owners_email
                    ON users.restaurant_owners (email);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no-op: this is a recovery migration for an environment-drift
            // scenario. Dropping the table here would cascade-remove restaurant.owner_id
            // links on rollback and is not desirable.
        }
    }
}
