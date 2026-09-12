using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAIAnalyses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_analyses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    occurrence_id = table.Column<int>(type: "integer", nullable: false),
                    recommended_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    recommended_priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    recommends_police = table.Column<bool>(type: "boolean", nullable: false),
                    recommends_samu = table.Column<bool>(type: "boolean", nullable: false),
                    recommends_fire_department = table.Column<bool>(type: "boolean", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_analyses", x => x.id);
                    table.CheckConstraint("ck_ai_analyses_recommended_priority", "recommended_priority IN ('BAIXA', 'MEDIA', 'ALTA', 'CRITICA')");
                    table.CheckConstraint("ck_ai_analyses_recommended_type", "recommended_type IN ('ACIDENTE_TRANSITO', 'INCENDIO', 'AGRESSAO', 'ROUBO', 'FERIMENTO', 'MAL_SUBITO', 'PESSOA_DESAPARECIDA', 'RESGATE', 'OUTROS')");
                    table.CheckConstraint("ck_ai_analyses_services", "recommends_police OR recommends_samu OR recommends_fire_department");
                    table.ForeignKey(
                        name: "FK_ai_analyses_occurrences_occurrence_id",
                        column: x => x.occurrence_id,
                        principalTable: "occurrences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_analyses_occurrence_id_created_at",
                table: "ai_analyses",
                columns: new[] { "occurrence_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_analyses");
        }
    }
}
