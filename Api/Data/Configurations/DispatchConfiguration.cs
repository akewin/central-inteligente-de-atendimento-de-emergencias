using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api.Models.Enums;

namespace Api.Data.Configurations;

public sealed class DispatchConfiguration : IEntityTypeConfiguration<Dispatch>
{
    public void Configure(EntityTypeBuilder<Dispatch> builder)
    {
        builder.ToTable("dispatches", table => table.HasCheckConstraint(
            "ck_dispatches_status",
            "status IN ('ATRIBUIDO', 'ACEITO', 'NO_LOCAL', 'EM_ATENDIMENTO', 'CONCLUIDO', 'CANCELADO')"));
        builder.HasKey(dispatch => dispatch.Id);
        builder.Property(dispatch => dispatch.Id).HasColumnName("id");
        builder.Property(dispatch => dispatch.OccurrenceId).HasColumnName("occurrence_id").IsRequired();
        builder.Property(dispatch => dispatch.UnitId).HasColumnName("unit_id").IsRequired();
        builder.Property(dispatch => dispatch.ConfirmedByUserId).HasColumnName("confirmed_by_user_id").IsRequired();
        builder.Property(dispatch => dispatch.Status).HasColumnName("status").HasConversion<string>()
            .HasMaxLength(20).HasDefaultValue(DispatchStatus.ATRIBUIDO).IsRequired();
        builder.Property(dispatch => dispatch.AcceptedAt).HasColumnName("accepted_at");
        builder.Property(dispatch => dispatch.ArrivedAt).HasColumnName("arrived_at");
        builder.Property(dispatch => dispatch.ServiceStartedAt).HasColumnName("service_started_at");
        builder.Property(dispatch => dispatch.CompletedAt).HasColumnName("completed_at");
        builder.Property(dispatch => dispatch.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(dispatch => dispatch.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(dispatch => dispatch.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(dispatch => new { dispatch.OccurrenceId, dispatch.UnitId })
            .IsUnique().HasDatabaseName("ux_dispatches_occurrence_unit");
        builder.HasIndex(dispatch => new { dispatch.OccurrenceId, dispatch.CreatedAt })
            .HasDatabaseName("ix_dispatches_occurrence_created_at");
        builder.HasIndex(dispatch => new { dispatch.UnitId, dispatch.CreatedAt })
            .HasDatabaseName("ix_dispatches_unit_created_at");
        builder.HasIndex(dispatch => dispatch.ConfirmedByUserId)
            .HasDatabaseName("ix_dispatches_confirmed_by_user_id");
        builder.HasIndex(dispatch => new { dispatch.OccurrenceId, dispatch.Status })
            .HasDatabaseName("ix_dispatches_occurrence_status");
        builder.HasIndex(dispatch => new { dispatch.UnitId, dispatch.Status, dispatch.CreatedAt })
            .HasDatabaseName("ix_dispatches_unit_status_created_at");
        builder.HasIndex(dispatch => dispatch.UnitId).IsUnique()
            .HasFilter("status NOT IN ('CONCLUIDO', 'CANCELADO')")
            .HasDatabaseName("ux_dispatches_active_unit");

        builder.HasOne(dispatch => dispatch.Occurrence).WithMany(occurrence => occurrence.Dispatches)
            .HasForeignKey(dispatch => dispatch.OccurrenceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(dispatch => dispatch.Unit).WithMany(unit => unit.Dispatches)
            .HasForeignKey(dispatch => dispatch.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(dispatch => dispatch.ConfirmedByUser).WithMany(user => user.ConfirmedDispatches)
            .HasForeignKey(dispatch => dispatch.ConfirmedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
