using Kinesis.Poc.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kinesis.Poc.Api.Data;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients", table =>
        {
            table.HasCheckConstraint("CK_Patients_Status", "[Status] IN (N'Active', N'Inactive')");
            table.HasCheckConstraint("CK_Patients_DocumentPair",
                "([DocumentType] IS NULL AND [DocumentNumber] IS NULL) OR " +
                "([DocumentType] IS NOT NULL AND [DocumentNumber] IS NOT NULL " +
                "AND LEN(LTRIM(RTRIM([DocumentType]))) > 0 " +
                "AND LEN(LTRIM(RTRIM([DocumentNumber]))) > 0)");
            table.HasCheckConstraint("CK_Patients_FirstName",
                "LEN(LTRIM(RTRIM([FirstName]))) > 0");
            table.HasCheckConstraint("CK_Patients_LastName",
                "LEN(LTRIM(RTRIM([LastName]))) > 0");
        });

        builder.HasKey(patient => patient.Id);
        builder.Property(patient => patient.Id)
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever();

        builder.Property(patient => patient.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(patient => patient.LastName).IsRequired().HasMaxLength(100);
        builder.Property(patient => patient.DocumentType).HasMaxLength(30);
        builder.Property(patient => patient.DocumentNumber).HasMaxLength(30);
        builder.Property(patient => patient.BirthDate).IsRequired().HasColumnType("date");
        builder.Property(patient => patient.Email).HasMaxLength(150);
        builder.Property(patient => patient.Phone).HasMaxLength(30);
        builder.Property(patient => patient.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(8);
        builder.Property(patient => patient.CreatedAt).IsRequired().HasColumnType("datetimeoffset");
        builder.Property(patient => patient.UpdatedAt).IsRequired().HasColumnType("datetimeoffset");

        builder.HasIndex(patient => new { patient.DocumentType, patient.DocumentNumber })
            .HasDatabaseName("UX_Patients_Document")
            .IsUnique()
            .HasFilter("[DocumentType] IS NOT NULL AND [DocumentNumber] IS NOT NULL");
    }
}
