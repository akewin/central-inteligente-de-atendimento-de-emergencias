using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table => table.HasCheckConstraint("ck_users_operational_assignment", "unit_id IS NULL OR hospital_id IS NULL"));

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
               .HasColumnName("id");

        builder.Property(u => u.Username)
               .HasColumnName("username")
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(u => u.Email)
               .HasColumnName("email")
               .IsRequired()
               .HasMaxLength(120);

        builder.Property(u => u.Password)
               .HasColumnName("password_hash")
               .IsRequired();

        builder.Property(u => u.FullName)
               .HasColumnName("full_name")
               .IsRequired()
               .HasMaxLength(120);

        builder.Property(u => u.Active)
               .HasColumnName("active")
               .IsRequired();

        builder.Property(u => u.UnitId).HasColumnName("unit_id");
        builder.Property(u => u.HospitalId).HasColumnName("hospital_id");

        builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(u => u.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired();

        builder.HasIndex(u => u.Username)
               .IsUnique()
               .HasDatabaseName("ux_users_username");

        builder.HasIndex(u => u.Email)
               .IsUnique()
               .HasDatabaseName("ux_users_email");

        builder.HasIndex(u => u.UnitId).IsUnique().HasFilter("unit_id IS NOT NULL")
               .HasDatabaseName("ux_users_unit_id");

        builder.HasIndex(u => u.HospitalId).HasFilter("hospital_id IS NOT NULL")
               .HasDatabaseName("ix_users_hospital_id");

        builder.HasOne(u => u.Unit).WithOne(unit => unit.OperationalUser)
               .HasForeignKey<User>(u => u.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(u => u.Hospital).WithMany(hospital => hospital.Users)
               .HasForeignKey(u => u.HospitalId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AccessPermissions)
               .WithOne(ap => ap.User)
               .HasForeignKey(ap => ap.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
