using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyServicesAndUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "emergency_services",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    emergency_number = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emergency_services", x => x.id);
                    table.CheckConstraint("ck_emergency_services_mapping", "(type = 'POLICIA' AND emergency_number = '190') OR (type = 'SAMU' AND emergency_number = '192') OR (type = 'BOMBEIROS' AND emergency_number = '193')");
                    table.CheckConstraint("ck_emergency_services_number", "emergency_number IN ('190', '192', '193')");
                    table.CheckConstraint("ck_emergency_services_type", "type IN ('POLICIA', 'SAMU', 'BOMBEIROS')");
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    emergency_service_id = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DISPONIVEL"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.id);
                    table.CheckConstraint("ck_units_latitude", "latitude BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_units_longitude", "longitude BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_units_status", "status IN ('DISPONIVEL', 'DESLOCAMENTO', 'EM_ATENDIMENTO', 'INDISPONIVEL')");
                    table.ForeignKey(
                        name: "FK_units_emergency_services_emergency_service_id",
                        column: x => x.emergency_service_id,
                        principalTable: "emergency_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_emergency_services_number",
                table: "emergency_services",
                column: "emergency_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_emergency_services_type",
                table: "emergency_services",
                column: "type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_units_service_status",
                table: "units",
                columns: new[] { "emergency_service_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_units_normalized_name",
                table: "units",
                column: "normalized_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropTable(
                name: "emergency_services");
        }
    }
}
