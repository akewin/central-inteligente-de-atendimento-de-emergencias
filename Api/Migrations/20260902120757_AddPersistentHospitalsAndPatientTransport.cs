using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPersistentHospitalsAndPatientTransport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "hospital_id",
                table: "users",
                type: "integer",
                nullable: true);

            // Os identificadores antigos vinham do OpenStreetMap. Nome, endereço e
            // coordenadas permanecem na ocorrência como histórico, mas o ID externo
            // não pode ser convertido com segurança para a nova chave interna.
            migrationBuilder.DropColumn(name: "selected_hospital_id", table: "occurrences");
            migrationBuilder.AddColumn<int>(name: "selected_hospital_id", table: "occurrences", type: "integer", nullable: true);

            migrationBuilder.CreateTable(
                name: "hospitals",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(9,7)", precision: 9, scale: 7, nullable: false),
                    longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: false),
                    has_emergency_department = table.Column<bool>(type: "boolean", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hospitals", x => x.id);
                    table.CheckConstraint("ck_hospitals_city", "upper(city) = 'ARARAS'");
                    table.CheckConstraint("ck_hospitals_latitude", "latitude BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_hospitals_longitude", "longitude BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_hospitals_state", "upper(state) = 'SP'");
                });

            migrationBuilder.CreateTable(
                name: "hospital_wards",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hospital_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    total_beds = table.Column<int>(type: "integer", nullable: false),
                    occupied_beds = table.Column<int>(type: "integer", nullable: false),
                    reserved_beds = table.Column<int>(type: "integer", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hospital_wards", x => x.id);
                    table.CheckConstraint("ck_hospital_wards_capacity", "total_beds >= 0 AND occupied_beds >= 0 AND reserved_beds >= 0 AND occupied_beds + reserved_beds <= total_beds");
                    table.ForeignKey(
                        name: "FK_hospital_wards_hospitals_hospital_id",
                        column: x => x.hospital_id,
                        principalTable: "hospitals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_transports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    occurrence_id = table.Column<int>(type: "integer", nullable: false),
                    requested_by_dispatch_id = table.Column<int>(type: "integer", nullable: false),
                    samu_dispatch_id = table.Column<int>(type: "integer", nullable: true),
                    hospital_id = table.Column<int>(type: "integer", nullable: true),
                    hospital_ward_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    operational_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    hospital_notified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    transport_started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_transports", x => x.id);
                    table.CheckConstraint("ck_patient_transports_status", "status IN ('AGUARDANDO_CENTRAL','AGUARDANDO_DESTINO','HOSPITAL_AVISADO','EM_TRANSPORTE','RECEBIDO','CANCELADO')");
                    table.ForeignKey(
                        name: "FK_patient_transports_dispatches_requested_by_dispatch_id",
                        column: x => x.requested_by_dispatch_id,
                        principalTable: "dispatches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_patient_transports_dispatches_samu_dispatch_id",
                        column: x => x.samu_dispatch_id,
                        principalTable: "dispatches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_patient_transports_hospital_wards_hospital_ward_id",
                        column: x => x.hospital_ward_id,
                        principalTable: "hospital_wards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_patient_transports_hospitals_hospital_id",
                        column: x => x.hospital_id,
                        principalTable: "hospitals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_patient_transports_occurrences_occurrence_id",
                        column: x => x.occurrence_id,
                        principalTable: "occurrences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_hospital_id",
                table: "users",
                column: "hospital_id",
                filter: "hospital_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_occurrences_selected_hospital_id",
                table: "occurrences",
                column: "selected_hospital_id");

            migrationBuilder.CreateIndex(
                name: "ux_hospital_wards_name",
                table: "hospital_wards",
                columns: new[] { "hospital_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_hospitals_name",
                table: "hospitals",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_transports_hospital_id",
                table: "patient_transports",
                column: "hospital_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_transports_hospital_ward_id",
                table: "patient_transports",
                column: "hospital_ward_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_transports_requested_by_dispatch_id",
                table: "patient_transports",
                column: "requested_by_dispatch_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_transports_samu_dispatch_id",
                table: "patient_transports",
                column: "samu_dispatch_id");

            migrationBuilder.CreateIndex(
                name: "ux_patient_transports_active_occurrence",
                table: "patient_transports",
                column: "occurrence_id",
                unique: true,
                filter: "status NOT IN ('RECEBIDO','CANCELADO')");

            migrationBuilder.AddForeignKey(
                name: "FK_occurrences_hospitals_selected_hospital_id",
                table: "occurrences",
                column: "selected_hospital_id",
                principalTable: "hospitals",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_hospitals_hospital_id",
                table: "users",
                column: "hospital_id",
                principalTable: "hospitals",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_occurrences_hospitals_selected_hospital_id",
                table: "occurrences");

            migrationBuilder.DropForeignKey(
                name: "FK_users_hospitals_hospital_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "patient_transports");

            migrationBuilder.DropTable(
                name: "hospital_wards");

            migrationBuilder.DropTable(
                name: "hospitals");

            migrationBuilder.DropIndex(
                name: "ix_users_hospital_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_occurrences_selected_hospital_id",
                table: "occurrences");

            migrationBuilder.DropColumn(
                name: "hospital_id",
                table: "users");

            migrationBuilder.DropColumn(name: "selected_hospital_id", table: "occurrences");
            migrationBuilder.AddColumn<string>(name: "selected_hospital_id", table: "occurrences", type: "character varying(100)", maxLength: 100, nullable: true);
        }
    }
}
