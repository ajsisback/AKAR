using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Akar.Infrastructure.Persistence.Configurations;

public class ProjectContractConfiguration : IEntityTypeConfiguration<ProjectContract>
{
    public void Configure(EntityTypeBuilder<ProjectContract> builder)
    {
        builder.ToTable("project_contracts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(c => c.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(c => c.ContractTemplateId)
            .HasColumnName("contract_template_id")
            .IsRequired();

        builder.Property(c => c.ContractNumber)
            .HasColumnName("contract_number")
            .HasMaxLength(50);

        builder.Property(c => c.ContractTitle)
            .HasColumnName("contract_title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.ContractType)
            .HasColumnName("contract_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.PartyName)
            .HasColumnName("party_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.PartyPhone)
            .HasColumnName("party_phone")
            .HasMaxLength(30);

        builder.Property(c => c.PartyNationalId)
            .HasColumnName("party_national_id")
            .HasMaxLength(50);

        builder.Property(c => c.ContractValue)
            .HasColumnName("contract_value")
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.StartDate)
            .HasColumnName("start_date");

        builder.Property(c => c.EndDate)
            .HasColumnName("end_date");

        builder.Property(c => c.ContractDataJson)
            .HasColumnName("contract_data_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.PdfFileId)
            .HasColumnName("pdf_file_id");

        builder.Property(c => c.SignedFileId)
            .HasColumnName("signed_file_id");

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(c => c.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(c => c.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // Indexes
        builder.HasIndex(c => c.ProjectId);
        builder.HasIndex(c => c.OwnerId);
        builder.HasIndex(c => c.ContractTemplateId);
        builder.HasIndex(c => c.ContractType);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.IsDeleted);
        builder.HasIndex(c => c.CreatedAtUtc);

        // Relationships
        builder.HasOne(c => c.Project)
            .WithMany()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ContractTemplate)
            .WithMany()
            .HasForeignKey(c => c.ContractTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PdfFile)
            .WithMany()
            .HasForeignKey(c => c.PdfFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.SignedFile)
            .WithMany()
            .HasForeignKey(c => c.SignedFileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
