using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public sealed class HospitalWardConfiguration : IEntityTypeConfiguration<HospitalWard>
{
    public void Configure(EntityTypeBuilder<HospitalWard> builder)
    {
        builder.ToTable("hospital_wards", table => table.HasCheckConstraint(
            "ck_hospital_wards_capacity", "total_beds >= 0 AND occupied_beds >= 0 AND reserved_beds >= 0 AND occupied_beds + reserved_beds <= total_beds"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.HospitalId).HasColumnName("hospital_id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.TotalBeds).HasColumnName("total_beds");
        builder.Property(x => x.OccupiedBeds).HasColumnName("occupied_beds");
        builder.Property(x => x.ReservedBeds).HasColumnName("reserved_beds");
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsConcurrencyToken().IsRequired();
        builder.Ignore(x => x.AvailableBeds);
        builder.HasIndex(x => new { x.HospitalId, x.Name }).IsUnique().HasDatabaseName("ux_hospital_wards_name");
        builder.HasOne(x => x.Hospital).WithMany(x => x.Wards).HasForeignKey(x => x.HospitalId).OnDelete(DeleteBehavior.Cascade);
    }
}
