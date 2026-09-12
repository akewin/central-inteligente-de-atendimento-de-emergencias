using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOccurrenceStructuredAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address_reference",
                table: "occurrences",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "occurrences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "complement",
                table: "occurrences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "geocoding_source",
                table: "occurrences",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "neighborhood",
                table: "occurrences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "number",
                table: "occurrences",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postal_code",
                table: "occurrences",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "occurrences",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street",
                table: "occurrences",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_occurrences_postal_code",
                table: "occurrences",
                column: "postal_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_occurrences_postal_code",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "address_reference",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "city",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "complement",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "geocoding_source",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "neighborhood",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "number",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "postal_code",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "state",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "street",
                table: "occurrences");
        }
    }
}
