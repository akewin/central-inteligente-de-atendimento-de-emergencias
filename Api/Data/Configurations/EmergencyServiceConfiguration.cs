using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public sealed class EmergencyServiceConfiguration : IEntityTypeConfiguration<EmergencyService>
{
    public void Configure(EntityTypeBuilder<EmergencyService> builder)
    {
        builder.ToTable("emergency_services", table =>
        {
            table.HasCheckConstraint("ck_emergency_services_type", "type IN ('GUARDA_CIVIL', 'SAMU', 'DEFESA_CIVIL')");
            table.HasCheckConstraint("ck_emergency_services_number", "emergency_number IN ('153', '192', '199')");
            table.HasCheckConstraint(
                "ck_emergency_services_mapping",
                "(type = 'GUARDA_CIVIL' AND emergency_number = '153') OR " +
                "(type = 'SAMU' AND emergency_number = '192') OR " +
                "(type = 'DEFESA_CIVIL' AND emergency_number = '199')");
        });

        builder.HasKey(service => service.Id);
        builder.Property(service => service.Id).HasColumnName("id");
        builder.Property(service => service.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(service => service.EmergencyNumber).HasColumnName("emergency_number").HasMaxLength(3).IsRequired();
        builder.Property(service => service.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
        builder.Property(service => service.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(service => service.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(service => service.Type).IsUnique().HasDatabaseName("ux_emergency_services_type");
        builder.HasIndex(service => service.EmergencyNumber).IsUnique().HasDatabaseName("ux_emergency_services_number");
    }
}
