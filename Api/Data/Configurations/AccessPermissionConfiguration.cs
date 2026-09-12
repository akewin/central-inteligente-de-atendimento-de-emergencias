using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class AccessPermissionConfiguration : IEntityTypeConfiguration<AccessPermission>
{
    public void Configure(EntityTypeBuilder<AccessPermission> builder)
    {
        builder.ToTable("access_permissions");

        builder.HasKey(ap => ap.Id);

        builder.Property(ap => ap.Id)
               .HasColumnName("id");

        builder.Property(ap => ap.UserId)
               .HasColumnName("user_id")
               .IsRequired();

        builder.Property(ap => ap.SystemResourceId)
               .HasColumnName("system_resource_id")
               .IsRequired();

        builder.Property(ap => ap.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(ap => ap.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired();

        builder.HasIndex(ap => new { ap.UserId, ap.SystemResourceId })
               .IsUnique()
               .HasDatabaseName("ux_access_permissions_user_resource");

        builder.HasOne(ap => ap.User)
               .WithMany(u => u.AccessPermissions)
               .HasForeignKey(ap => ap.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ap => ap.SystemResource)
               .WithMany(r => r.AccessPermissions)
               .HasForeignKey(ap => ap.SystemResourceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
