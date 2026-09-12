using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOccurrenceServiceConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "fire_department_confirmed",
                table: "occurrences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "police_confirmed",
                table: "occurrences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "samu_confirmed",
                table: "occurrences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "services_confirmed_at",
                table: "occurrences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "services_confirmed_by_user_id",
                table: "occurrences",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_occurrences_services_confirmed_by_user_id",
                table: "occurrences",
                column: "services_confirmed_by_user_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_occurrences_service_confirmation",
                table: "occurrences",
                sql: "(services_confirmed_by_user_id IS NULL AND services_confirmed_at IS NULL AND NOT police_confirmed AND NOT samu_confirmed AND NOT fire_department_confirmed) OR (services_confirmed_by_user_id IS NOT NULL AND services_confirmed_at IS NOT NULL AND (police_confirmed OR samu_confirmed OR fire_department_confirmed))");

            migrationBuilder.AddForeignKey(
                name: "FK_occurrences_users_services_confirmed_by_user_id",
                table: "occurrences",
                column: "services_confirmed_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_occurrences_users_services_confirmed_by_user_id",
                table: "occurrences");

            migrationBuilder.DropIndex(
                name: "IX_occurrences_services_confirmed_by_user_id",
                table: "occurrences");

            migrationBuilder.DropCheckConstraint(
                name: "ck_occurrences_service_confirmation",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "fire_department_confirmed",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "police_confirmed",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "samu_confirmed",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "services_confirmed_at",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "services_confirmed_by_user_id",
                table: "occurrences");
        }
    }
}
