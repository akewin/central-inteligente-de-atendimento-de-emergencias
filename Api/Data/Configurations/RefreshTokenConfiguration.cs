using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                .HasColumnName("id");

            builder.Property(rt => rt.TokenHash)
                .HasColumnName("token_hash")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(rt => rt.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            builder.Property(rt => rt.IsRevoked)
                .HasColumnName("is_revoked")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(rt => rt.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(rt => rt.RevokedAt)
                .HasColumnName("revoked_at");

            builder.Property(rt => rt.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rt => rt.TokenHash)
                .IsUnique();

            builder.HasIndex(rt => rt.UserId);
        }
    }
}
