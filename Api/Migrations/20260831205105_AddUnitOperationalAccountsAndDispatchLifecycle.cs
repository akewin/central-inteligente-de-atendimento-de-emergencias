using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitOperationalAccountsAndDispatchLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_units_status",
                table: "units");

            migrationBuilder.AddColumn<int>(
                name: "unit_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "accepted_at",
                table: "dispatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "arrived_at",
                table: "dispatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "dispatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "dispatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "service_started_at",
                table: "dispatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "dispatches",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ATRIBUIDO");

            migrationBuilder.Sql("""
                UPDATE dispatches AS d
                SET status = CASE
                    WHEN o.status = 'FINALIZADA' THEN 'CONCLUIDO'
                    WHEN o.status = 'CANCELADA' THEN 'CANCELADO'
                    WHEN o.status = 'EM_ATENDIMENTO' THEN 'EM_ATENDIMENTO'
                    ELSE 'ACEITO'
                END,
                accepted_at = CASE WHEN o.status NOT IN ('FINALIZADA', 'CANCELADA') THEN d.created_at ELSE NULL END,
                service_started_at = CASE WHEN o.status = 'EM_ATENDIMENTO' THEN d.updated_at ELSE NULL END,
                completed_at = CASE WHEN o.status = 'FINALIZADA' THEN d.updated_at ELSE NULL END,
                cancelled_at = CASE WHEN o.status = 'CANCELADA' THEN d.updated_at ELSE NULL END
                FROM occurrences AS o
                WHERE o.id = d.occurrence_id;
                """);

            // Protege a migração contra valores legados/defaults produzidos por versões
            // intermediárias do protótipo antes de ativar a allowlist no banco.
            migrationBuilder.Sql("""
                UPDATE dispatches
                SET status = 'ACEITO',
                    accepted_at = COALESCE(accepted_at, created_at)
                WHERE status NOT IN ('ATRIBUIDO', 'ACEITO', 'NO_LOCAL', 'EM_ATENDIMENTO', 'CONCLUIDO', 'CANCELADO');
                """);

            migrationBuilder.CreateIndex(
                name: "ux_users_unit_id",
                table: "users",
                column: "unit_id",
                unique: true,
                filter: "unit_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_units_status",
                table: "units",
                sql: "status IN ('DISPONIVEL', 'RESERVADA', 'DESLOCAMENTO', 'EM_ATENDIMENTO', 'INDISPONIVEL')");

            migrationBuilder.CreateIndex(
                name: "ix_dispatches_occurrence_status",
                table: "dispatches",
                columns: new[] { "occurrence_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_dispatches_unit_status_created_at",
                table: "dispatches",
                columns: new[] { "unit_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_dispatches_active_unit",
                table: "dispatches",
                column: "unit_id",
                unique: true,
                filter: "status NOT IN ('CONCLUIDO', 'CANCELADO')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_dispatches_status",
                table: "dispatches",
                sql: "status IN ('ATRIBUIDO', 'ACEITO', 'NO_LOCAL', 'EM_ATENDIMENTO', 'CONCLUIDO', 'CANCELADO')");

            migrationBuilder.AddForeignKey(
                name: "FK_users_units_unit_id",
                table: "users",
                column: "unit_id",
                principalTable: "units",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_units_unit_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ux_users_unit_id",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "ck_units_status",
                table: "units");

            migrationBuilder.DropIndex(
                name: "ix_dispatches_occurrence_status",
                table: "dispatches");

            migrationBuilder.DropIndex(
                name: "ix_dispatches_unit_status_created_at",
                table: "dispatches");

            migrationBuilder.DropIndex(
                name: "ux_dispatches_active_unit",
                table: "dispatches");

            migrationBuilder.DropCheckConstraint(
                name: "ck_dispatches_status",
                table: "dispatches");

            migrationBuilder.DropColumn(
                name: "unit_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "accepted_at",
                table: "dispatches");

            migrationBuilder.DropColumn(
                name: "arrived_at",
                table: "dispatches");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "dispatches");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "dispatches");

            migrationBuilder.DropColumn(
                name: "service_started_at",
                table: "dispatches");

            migrationBuilder.DropColumn(
                name: "status",
                table: "dispatches");

            migrationBuilder.Sql("UPDATE units SET status = 'DISPONIVEL' WHERE status = 'RESERVADA';");

            migrationBuilder.AddCheckConstraint(
                name: "ck_units_status",
                table: "units",
                sql: "status IN ('DISPONIVEL', 'DESLOCAMENTO', 'EM_ATENDIMENTO', 'INDISPONIVEL')");
        }
    }
}
