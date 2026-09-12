using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class OccurrenceConfiguration : IEntityTypeConfiguration<Occurrence>
{
    public void Configure(EntityTypeBuilder<Occurrence> builder)
    {
        builder.ToTable("occurrences", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "ck_occurrences_latitude",
                "latitude BETWEEN -90 AND 90");
            tableBuilder.HasCheckConstraint(
                "ck_occurrences_longitude",
                "longitude BETWEEN -180 AND 180");
            tableBuilder.HasCheckConstraint(
                "ck_occurrences_status",
                "status IN ('ABERTA', 'EM_ANALISE', 'AGUARDANDO_CONFIRMACAO', 'DESPACHADA', 'EM_ATENDIMENTO', 'FINALIZADA', 'CANCELADA')");
            tableBuilder.HasCheckConstraint(
                "ck_occurrences_confirmed_type",
                "confirmed_type IS NULL OR confirmed_type IN ('ACIDENTE_TRANSITO', 'INCENDIO', 'AGRESSAO', 'ROUBO', 'FERIMENTO', 'MAL_SUBITO', 'PESSOA_DESAPARECIDA', 'RESGATE', 'OUTROS')");
            tableBuilder.HasCheckConstraint(
                "ck_occurrences_confirmed_priority",
                "confirmed_priority IS NULL OR confirmed_priority IN ('BAIXA', 'MEDIA', 'ALTA', 'CRITICA')");
            tableBuilder.HasCheckConstraint(
                "ck_occurrences_service_confirmation",
                "(services_confirmed_by_user_id IS NULL AND services_confirmed_at IS NULL AND " +
                "NOT police_confirmed AND NOT samu_confirmed AND NOT fire_department_confirmed) OR " +
                "(services_confirmed_by_user_id IS NOT NULL AND services_confirmed_at IS NOT NULL AND " +
                "(police_confirmed OR samu_confirmed OR fire_department_confirmed))");
        });

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
               .HasColumnName("id");

        builder.Property(o => o.Description)
               .HasColumnName("description")
               .HasMaxLength(4000)
               .IsRequired();

        builder.Property(o => o.LocationDescription)
               .HasColumnName("location_description")
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(o => o.PostalCode).HasColumnName("postal_code").HasMaxLength(8);
        builder.Property(o => o.Street).HasColumnName("street").HasMaxLength(200);
        builder.Property(o => o.Number).HasColumnName("number").HasMaxLength(20);
        builder.Property(o => o.Complement).HasColumnName("complement").HasMaxLength(100);
        builder.Property(o => o.Neighborhood).HasColumnName("neighborhood").HasMaxLength(100);
        builder.Property(o => o.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(o => o.State).HasColumnName("state").HasMaxLength(2);
        builder.Property(o => o.AddressReference).HasColumnName("address_reference").HasMaxLength(200);
        builder.Property(o => o.GeocodingSource).HasColumnName("geocoding_source").HasMaxLength(30);

        builder.Property(o => o.Latitude)
               .HasColumnName("latitude")
               .HasPrecision(8, 6)
               .IsRequired();

        builder.Property(o => o.Longitude)
               .HasColumnName("longitude")
               .HasPrecision(9, 6)
               .IsRequired();

        builder.Property(o => o.SelectedHospitalId).HasColumnName("selected_hospital_id");
        builder.Property(o => o.SelectedHospitalName).HasColumnName("selected_hospital_name").HasMaxLength(250);
        builder.Property(o => o.SelectedHospitalAddress).HasColumnName("selected_hospital_address").HasMaxLength(500);
        builder.Property(o => o.SelectedHospitalLatitude).HasColumnName("selected_hospital_latitude").HasPrecision(9, 7);
        builder.Property(o => o.SelectedHospitalLongitude).HasColumnName("selected_hospital_longitude").HasPrecision(10, 7);
        builder.Property(o => o.HospitalSelectedAt).HasColumnName("hospital_selected_at");

        builder.Property(o => o.ConfirmedType)
               .HasColumnName("confirmed_type")
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired(false);

        builder.Property(o => o.ConfirmedPriority)
               .HasColumnName("confirmed_priority")
               .HasConversion<string>()
               .HasMaxLength(10)
               .IsRequired(false);

        builder.Property(o => o.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(30)
               .HasDefaultValue(OccurrenceStatus.ABERTA)
               .IsRequired();

        builder.Property(o => o.PoliceConfirmed)
               .HasColumnName("police_confirmed")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(o => o.SamuConfirmed)
               .HasColumnName("samu_confirmed")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(o => o.FireDepartmentConfirmed)
               .HasColumnName("fire_department_confirmed")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(o => o.ServicesConfirmedByUserId)
               .HasColumnName("services_confirmed_by_user_id")
               .IsRequired(false);

        builder.Property(o => o.ServicesConfirmedAt)
               .HasColumnName("services_confirmed_at")
               .IsRequired(false);

        builder.Property(o => o.CreatedByUserId)
               .HasColumnName("created_by_user_id")
               .IsRequired();

        builder.Property(o => o.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(o => o.UpdatedAt)
               .HasColumnName("updated_at")
               .IsConcurrencyToken()
               .IsRequired();

        builder.HasIndex(o => new { o.Status, o.CreatedAt })
               .HasDatabaseName("ix_occurrences_status_created_at");

        builder.HasIndex(o => new { o.CreatedByUserId, o.CreatedAt })
               .HasDatabaseName("ix_occurrences_created_by_user_id_created_at");

        builder.HasIndex(o => o.PostalCode).HasDatabaseName("ix_occurrences_postal_code");

        builder.HasOne(o => o.CreatedByUser)
               .WithMany(u => u.Occurrences)
               .HasForeignKey(o => o.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ServicesConfirmedByUser)
               .WithMany(u => u.ConfirmedServiceOccurrences)
               .HasForeignKey(o => o.ServicesConfirmedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.SelectedHospital).WithMany()
               .HasForeignKey(o => o.SelectedHospitalId).OnDelete(DeleteBehavior.Restrict);
    }
}
