using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOccurrences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "occurrences",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    location_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    confirmed_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    confirmed_priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "ABERTA"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_occurrences", x => x.id);
                    table.CheckConstraint("ck_occurrences_confirmed_priority", "confirmed_priority IS NULL OR confirmed_priority IN ('BAIXA', 'MEDIA', 'ALTA', 'CRITICA')");
                    table.CheckConstraint("ck_occurrences_confirmed_type", "confirmed_type IS NULL OR confirmed_type IN ('ACIDENTE_TRANSITO', 'INCENDIO', 'AGRESSAO', 'ROUBO', 'FERIMENTO', 'MAL_SUBITO', 'PESSOA_DESAPARECIDA', 'RESGATE', 'OUTROS')");
                    table.CheckConstraint("ck_occurrences_latitude", "latitude BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_occurrences_longitude", "longitude BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_occurrences_status", "status IN ('ABERTA', 'EM_ANALISE', 'AGUARDANDO_CONFIRMACAO', 'DESPACHADA', 'EM_ATENDIMENTO', 'FINALIZADA', 'CANCELADA')");
                    table.ForeignKey(
                        name: "FK_occurrences_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_occurrences_created_by_user_id_created_at",
                table: "occurrences",
                columns: new[] { "created_by_user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_occurrences_status_created_at",
                table: "occurrences",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "occurrences");
        }
    }
}
