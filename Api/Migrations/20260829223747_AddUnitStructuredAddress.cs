using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitStructuredAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address_reference",
                table: "units",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "units",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "complement",
                table: "units",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "geocoding_source",
                table: "units",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "neighborhood",
                table: "units",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "number",
                table: "units",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postal_code",
                table: "units",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "units",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street",
                table: "units",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_units_postal_code",
                table: "units",
                column: "postal_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_units_postal_code",
                table: "units");

            migrationBuilder.DropColumn(
                name: "address_reference",
                table: "units");

            migrationBuilder.DropColumn(
                name: "city",
                table: "units");

            migrationBuilder.DropColumn(
                name: "complement",
                table: "units");

            migrationBuilder.DropColumn(
                name: "geocoding_source",
                table: "units");

            migrationBuilder.DropColumn(
                name: "neighborhood",
                table: "units");

            migrationBuilder.DropColumn(
                name: "number",
                table: "units");

            migrationBuilder.DropColumn(
                name: "postal_code",
                table: "units");

            migrationBuilder.DropColumn(
                name: "state",
                table: "units");

            migrationBuilder.DropColumn(
                name: "street",
                table: "units");
        }
    }
}
