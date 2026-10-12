using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations;

public partial class RenameEmergencyServices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        DropConstraints(migrationBuilder);
        migrationBuilder.Sql("""
            UPDATE emergency_services SET type = 'GUARDA_CIVIL', emergency_number = '153',
                display_name = 'Guarda Civil', updated_at = CURRENT_TIMESTAMP WHERE type = 'POLICIA';
            UPDATE emergency_services SET type = 'DEFESA_CIVIL', emergency_number = '199',
                display_name = 'Defesa Civil', updated_at = CURRENT_TIMESTAMP WHERE type = 'BOMBEIROS';
            UPDATE units AS u SET name = names.new_name, normalized_name = names.new_name,
                updated_at = CURRENT_TIMESTAMP
            FROM (VALUES ('PM-01', 'GC-01'), ('PM-02', 'GC-02'),
                         ('BM-01', 'DC-01'), ('BM-02', 'DC-02')) AS names(old_name, new_name)
            WHERE u.normalized_name = names.old_name
              AND NOT EXISTS (SELECT 1 FROM units other WHERE other.normalized_name = names.new_name);
            """);
        AddConstraints(migrationBuilder, "GUARDA_CIVIL", "153", "DEFESA_CIVIL", "199");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        DropConstraints(migrationBuilder);
        migrationBuilder.Sql("""
            UPDATE emergency_services SET type = 'POLICIA', emergency_number = '190',
                display_name = 'Polícia Militar', updated_at = CURRENT_TIMESTAMP WHERE type = 'GUARDA_CIVIL';
            UPDATE emergency_services SET type = 'BOMBEIROS', emergency_number = '193',
                display_name = 'Corpo de Bombeiros', updated_at = CURRENT_TIMESTAMP WHERE type = 'DEFESA_CIVIL';
            UPDATE units AS u SET name = names.new_name, normalized_name = names.new_name,
                updated_at = CURRENT_TIMESTAMP
            FROM (VALUES ('GC-01', 'PM-01'), ('GC-02', 'PM-02'),
                         ('DC-01', 'BM-01'), ('DC-02', 'BM-02')) AS names(old_name, new_name)
            WHERE u.normalized_name = names.old_name
              AND NOT EXISTS (SELECT 1 FROM units other WHERE other.normalized_name = names.new_name);
            """);
        AddConstraints(migrationBuilder, "POLICIA", "190", "BOMBEIROS", "193");
    }

    private static void DropConstraints(MigrationBuilder migrationBuilder)
    {
        foreach (var name in new[] { "ck_emergency_services_type", "ck_emergency_services_number", "ck_emergency_services_mapping" })
            migrationBuilder.DropCheckConstraint(name, "emergency_services");
    }

    private static void AddConstraints(MigrationBuilder migrationBuilder, string guard, string guardNumber, string defense, string defenseNumber)
    {
        migrationBuilder.AddCheckConstraint("ck_emergency_services_type", "emergency_services",
            $"type IN ('{guard}', 'SAMU', '{defense}')");
        migrationBuilder.AddCheckConstraint("ck_emergency_services_number", "emergency_services",
            $"emergency_number IN ('{guardNumber}', '192', '{defenseNumber}')");
        migrationBuilder.AddCheckConstraint("ck_emergency_services_mapping", "emergency_services",
            $"(type = '{guard}' AND emergency_number = '{guardNumber}') OR " +
            "(type = 'SAMU' AND emergency_number = '192') OR " +
            $"(type = '{defense}' AND emergency_number = '{defenseNumber}')");
    }
}
