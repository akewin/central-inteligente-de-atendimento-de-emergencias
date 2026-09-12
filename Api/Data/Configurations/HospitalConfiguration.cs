using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public sealed class HospitalConfiguration : IEntityTypeConfiguration<Hospital>
{
    public void Configure(EntityTypeBuilder<Hospital> builder)
    {
        builder.ToTable("hospitals", table =>
        {
            table.HasCheckConstraint("ck_hospitals_city", "upper(city) = 'ARARAS'");
            table.HasCheckConstraint("ck_hospitals_state", "upper(state) = 'SP'");
            table.HasCheckConstraint("ck_hospitals_latitude", "latitude BETWEEN -90 AND 90");
            table.HasCheckConstraint("ck_hospitals_longitude", "longitude BETWEEN -180 AND 180");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(8).IsRequired();
        builder.Property(x => x.Street).HasColumnName("street").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Number).HasColumnName("number").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Complement).HasColumnName("complement").HasMaxLength(100);
        builder.Property(x => x.Neighborhood).HasColumnName("neighborhood").HasMaxLength(100).IsRequired();
        builder.Property(x => x.City).HasColumnName("city").HasMaxLength(100).IsRequired();
        builder.Property(x => x.State).HasColumnName("state").HasMaxLength(2).IsRequired();
        builder.Property(x => x.Latitude).HasColumnName("latitude").HasPrecision(9, 7).IsRequired();
        builder.Property(x => x.Longitude).HasColumnName("longitude").HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.HasEmergencyDepartment).HasColumnName("has_emergency_department").IsRequired();
        builder.Property(x => x.Active).HasColumnName("active").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsConcurrencyToken().IsRequired();
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("ux_hospitals_name");
    }
}
