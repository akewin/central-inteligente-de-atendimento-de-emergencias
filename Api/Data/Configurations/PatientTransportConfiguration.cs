using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public sealed class PatientTransportConfiguration : IEntityTypeConfiguration<PatientTransport>
{
    public void Configure(EntityTypeBuilder<PatientTransport> builder)
    {
        builder.ToTable("patient_transports", table => table.HasCheckConstraint("ck_patient_transports_status",
            "status IN ('AGUARDANDO_CENTRAL','AGUARDANDO_DESTINO','HOSPITAL_AVISADO','EM_TRANSPORTE','RECEBIDO','CANCELADO')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OccurrenceId).HasColumnName("occurrence_id");
        builder.Property(x => x.RequestedByDispatchId).HasColumnName("requested_by_dispatch_id");
        builder.Property(x => x.SamuDispatchId).HasColumnName("samu_dispatch_id");
        builder.Property(x => x.HospitalId).HasColumnName("hospital_id");
        builder.Property(x => x.HospitalWardId).HasColumnName("hospital_ward_id");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.OperationalNotes).HasColumnName("operational_notes").HasMaxLength(1000);
        builder.Property(x => x.EstimatedMinutes).HasColumnName("estimated_minutes");
        builder.Property(x => x.HospitalNotifiedAt).HasColumnName("hospital_notified_at");
        builder.Property(x => x.AcknowledgedAt).HasColumnName("acknowledged_at");
        builder.Property(x => x.TransportStartedAt).HasColumnName("transport_started_at");
        builder.Property(x => x.ReceivedAt).HasColumnName("received_at");
        builder.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsConcurrencyToken().IsRequired();
        builder.HasIndex(x => x.OccurrenceId).HasFilter("status NOT IN ('RECEBIDO','CANCELADO')").IsUnique().HasDatabaseName("ux_patient_transports_active_occurrence");
        builder.HasOne(x => x.Occurrence).WithMany(x => x.PatientTransports).HasForeignKey(x => x.OccurrenceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RequestedByDispatch).WithMany().HasForeignKey(x => x.RequestedByDispatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SamuDispatch).WithMany().HasForeignKey(x => x.SamuDispatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Hospital).WithMany(x => x.Transports).HasForeignKey(x => x.HospitalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.HospitalWard).WithMany(x => x.Transports).HasForeignKey(x => x.HospitalWardId).OnDelete(DeleteBehavior.Restrict);
    }
}
