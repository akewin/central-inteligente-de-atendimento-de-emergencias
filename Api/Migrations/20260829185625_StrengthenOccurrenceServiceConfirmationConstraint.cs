using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class StrengthenOccurrenceServiceConfirmationConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_occurrences_service_confirmation",
                table: "occurrences");

            migrationBuilder.AddCheckConstraint(
                name: "ck_occurrences_service_confirmation",
                table: "occurrences",
                sql: "(services_confirmed_by_user_id IS NULL AND services_confirmed_at IS NULL AND NOT police_confirmed AND NOT samu_confirmed AND NOT fire_department_confirmed) OR (services_confirmed_by_user_id IS NOT NULL AND services_confirmed_at IS NOT NULL AND (police_confirmed OR samu_confirmed OR fire_department_confirmed))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_occurrences_service_confirmation",
                table: "occurrences");

            migrationBuilder.AddCheckConstraint(
                name: "ck_occurrences_service_confirmation",
                table: "occurrences",
                sql: "(services_confirmed_by_user_id IS NULL AND services_confirmed_at IS NULL) OR (services_confirmed_by_user_id IS NOT NULL AND services_confirmed_at IS NOT NULL AND (police_confirmed OR samu_confirmed OR fire_department_confirmed))");
        }
    }
}
