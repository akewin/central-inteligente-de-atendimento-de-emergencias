using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class SystemResourceConfiguration : IEntityTypeConfiguration<SystemResource>
{
    public void Configure(EntityTypeBuilder<SystemResource> builder)
    {
        builder.ToTable("system_resources");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
               .HasColumnName("id");

        builder.Property(r => r.Name)
               .HasColumnName("name")
               .IsRequired()
               .HasMaxLength(40);

        builder.Property(r => r.ExhibitionName)
               .HasColumnName("exhibition_name")
               .IsRequired()
               .HasMaxLength(120);

        builder.Property(r => r.Active)
               .HasColumnName("active")
               .IsRequired();

        builder.Property(r => r.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(r => r.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired();

        builder.HasIndex(r => r.Name)
               .IsUnique()
               .HasDatabaseName("ux_system_resources_name");

        builder.HasMany(r => r.AccessPermissions)
               .WithOne(ap => ap.SystemResource)
               .HasForeignKey(ap => ap.SystemResourceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
