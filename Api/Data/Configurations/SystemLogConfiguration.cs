using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.ToTable("system_logs");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Id)
               .HasColumnName("id");

        builder.Property(sl => sl.UserId)
               .HasColumnName("user_id")
               .IsRequired(false);

        builder.Property(sl => sl.Action)
               .HasColumnName("action")
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(sl => sl.Data)
               .HasColumnName("data");

        builder.Property(sl => sl.GeneratedBy)
               .HasColumnName("generated_by")
               .HasMaxLength(100);

        builder.Property(sl => sl.IpAddress)
               .HasColumnName("ip_address")
               .HasMaxLength(45); // IPv6 safe

        builder.Property(sl => sl.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.HasIndex(sl => sl.UserId)
               .HasDatabaseName("ix_system_logs_user_id");

        builder.HasOne(sl => sl.User)
               .WithMany()
               .HasForeignKey(sl => sl.UserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
