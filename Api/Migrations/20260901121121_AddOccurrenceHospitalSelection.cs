using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOccurrenceHospitalSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "hospital_selected_at",
                table: "occurrences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "selected_hospital_address",
                table: "occurrences",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "selected_hospital_id",
                table: "occurrences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "selected_hospital_latitude",
                table: "occurrences",
                type: "numeric(9,7)",
                precision: 9,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "selected_hospital_longitude",
                table: "occurrences",
                type: "numeric(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "selected_hospital_name",
                table: "occurrences",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hospital_selected_at",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "selected_hospital_address",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "selected_hospital_id",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "selected_hospital_latitude",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "selected_hospital_longitude",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "selected_hospital_name",
                table: "occurrences");
        }
    }
}
