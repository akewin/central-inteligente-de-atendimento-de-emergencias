using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("units", table =>
        {
            table.HasCheckConstraint("ck_units_latitude", "latitude BETWEEN -90 AND 90");
            table.HasCheckConstraint("ck_units_longitude", "longitude BETWEEN -180 AND 180");
            table.HasCheckConstraint(
                "ck_units_status",
                "status IN ('DISPONIVEL', 'RESERVADA', 'DESLOCAMENTO', 'EM_ATENDIMENTO', 'INDISPONIVEL')");
        });

        builder.HasKey(unit => unit.Id);
        builder.Property(unit => unit.Id).HasColumnName("id");
        builder.Property(unit => unit.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(unit => unit.NormalizedName).HasColumnName("normalized_name").HasMaxLength(50).IsRequired();
        builder.Property(unit => unit.EmergencyServiceId).HasColumnName("emergency_service_id").IsRequired();
        builder.Property(unit => unit.PostalCode).HasColumnName("postal_code").HasMaxLength(8);
        builder.Property(unit => unit.Street).HasColumnName("street").HasMaxLength(200);
        builder.Property(unit => unit.Number).HasColumnName("number").HasMaxLength(20);
        builder.Property(unit => unit.Complement).HasColumnName("complement").HasMaxLength(100);
        builder.Property(unit => unit.Neighborhood).HasColumnName("neighborhood").HasMaxLength(100);
        builder.Property(unit => unit.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(unit => unit.State).HasColumnName("state").HasMaxLength(2);
        builder.Property(unit => unit.AddressReference).HasColumnName("address_reference").HasMaxLength(200);
        builder.Property(unit => unit.GeocodingSource).HasColumnName("geocoding_source").HasMaxLength(30);
        builder.Property(unit => unit.Latitude).HasColumnName("latitude").HasPrecision(8, 6).IsRequired();
        builder.Property(unit => unit.Longitude).HasColumnName("longitude").HasPrecision(9, 6).IsRequired();
        builder.Property(unit => unit.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).HasDefaultValue(UnitStatus.DISPONIVEL).IsRequired();
        builder.Property(unit => unit.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(unit => unit.UpdatedAt).HasColumnName("updated_at").IsConcurrencyToken().IsRequired();

        builder.HasIndex(unit => unit.NormalizedName).IsUnique().HasDatabaseName("ux_units_normalized_name");
        builder.HasIndex(unit => new { unit.EmergencyServiceId, unit.Status }).HasDatabaseName("ix_units_service_status");
        builder.HasIndex(unit => unit.PostalCode).HasDatabaseName("ix_units_postal_code");
        builder.HasOne(unit => unit.EmergencyService).WithMany(service => service.Units)
            .HasForeignKey(unit => unit.EmergencyServiceId).OnDelete(DeleteBehavior.Restrict);
    }
}
