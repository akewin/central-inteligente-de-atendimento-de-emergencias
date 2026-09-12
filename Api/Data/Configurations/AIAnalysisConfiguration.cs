using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public sealed class AIAnalysisConfiguration : IEntityTypeConfiguration<AIAnalysis>
{
    public void Configure(EntityTypeBuilder<AIAnalysis> builder)
    {
        builder.ToTable("ai_analyses", table =>
        {
            table.HasCheckConstraint("ck_ai_analyses_recommended_type",
                "recommended_type IN ('ACIDENTE_TRANSITO', 'INCENDIO', 'AGRESSAO', 'ROUBO', 'FERIMENTO', 'MAL_SUBITO', 'PESSOA_DESAPARECIDA', 'RESGATE', 'OUTROS')");
            table.HasCheckConstraint("ck_ai_analyses_recommended_priority",
                "recommended_priority IN ('BAIXA', 'MEDIA', 'ALTA', 'CRITICA')");
            table.HasCheckConstraint("ck_ai_analyses_services",
                "recommends_police OR recommends_samu OR recommends_fire_department");
        });

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.OccurrenceId).HasColumnName("occurrence_id").IsRequired();
        builder.Property(a => a.RecommendedType).HasColumnName("recommended_type")
            .HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(a => a.RecommendedPriority).HasColumnName("recommended_priority")
            .HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(a => a.RecommendsPolice).HasColumnName("recommends_police").IsRequired();
        builder.Property(a => a.RecommendsSamu).HasColumnName("recommends_samu").IsRequired();
        builder.Property(a => a.RecommendsFireDepartment).HasColumnName("recommends_fire_department").IsRequired();
        builder.Property(a => a.Reason).HasColumnName("reason").HasMaxLength(1000).IsRequired();
        builder.Property(a => a.Provider).HasColumnName("provider").HasMaxLength(50).IsRequired();
        builder.Property(a => a.Model).HasColumnName("model").HasMaxLength(100).IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(a => new { a.OccurrenceId, a.CreatedAt })
            .HasDatabaseName("ix_ai_analyses_occurrence_id_created_at");
        builder.HasOne(a => a.Occurrence).WithMany(o => o.AIAnalyses)
            .HasForeignKey(a => a.OccurrenceId).OnDelete(DeleteBehavior.Restrict);
    }
}
