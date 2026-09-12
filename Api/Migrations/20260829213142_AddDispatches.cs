using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDispatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dispatches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    occurrence_id = table.Column<int>(type: "integer", nullable: false),
                    unit_id = table.Column<int>(type: "integer", nullable: false),
                    confirmed_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispatches", x => x.id);
                    table.ForeignKey(
                        name: "FK_dispatches_occurrences_occurrence_id",
                        column: x => x.occurrence_id,
                        principalTable: "occurrences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dispatches_units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dispatches_users_confirmed_by_user_id",
                        column: x => x.confirmed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dispatches_confirmed_by_user_id",
                table: "dispatches",
                column: "confirmed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_dispatches_occurrence_created_at",
                table: "dispatches",
                columns: new[] { "occurrence_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_dispatches_unit_created_at",
                table: "dispatches",
                columns: new[] { "unit_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_dispatches_occurrence_unit",
                table: "dispatches",
                columns: new[] { "occurrence_id", "unit_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispatches");
        }
    }
}
