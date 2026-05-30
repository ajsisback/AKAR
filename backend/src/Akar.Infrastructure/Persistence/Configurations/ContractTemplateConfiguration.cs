using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Akar.Infrastructure.Persistence.Configurations;

public class ContractTemplateConfiguration : IEntityTypeConfiguration<ContractTemplate>
{
    public void Configure(EntityTypeBuilder<ContractTemplate> builder)
    {
        builder.ToTable("contract_templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.TemplateCode)
            .HasColumnName("template_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.TemplateNameAr)
            .HasColumnName("template_name_ar")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.TemplateNameEn)
            .HasColumnName("template_name_en")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.ContractType)
            .HasColumnName("contract_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.DescriptionAr)
            .HasColumnName("description_ar")
            .HasMaxLength(2000);

        builder.Property(t => t.DescriptionEn)
            .HasColumnName("description_en")
            .HasMaxLength(2000);

        builder.Property(t => t.DefaultTermsJson)
            .HasColumnName("default_terms_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(t => t.RequiredFieldsJson)
            .HasColumnName("required_fields_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(t => t.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        // Indexes
        builder.HasIndex(t => t.TemplateCode).IsUnique();
        builder.HasIndex(t => t.ContractType);
        builder.HasIndex(t => t.IsActive);
    }
}
