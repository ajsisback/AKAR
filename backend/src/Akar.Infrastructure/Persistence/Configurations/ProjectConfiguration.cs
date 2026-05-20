using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Akar.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.HasIndex(p => p.OwnerId);

        builder.Property(p => p.ProjectName)
            .HasColumnName("project_name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.ProjectType)
            .HasColumnName("project_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(p => p.LocationText)
            .HasColumnName("location_text")
            .HasMaxLength(500);

        builder.Property(p => p.MapLink)
            .HasColumnName("map_link")
            .HasMaxLength(2000);

        builder.Property(p => p.CurrentStage)
            .HasColumnName("current_stage")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(CurrentStage.NotStarted);

        builder.Property(p => p.OptionalImageUrl)
            .HasColumnName("optional_image_url")
            .HasMaxLength(2000);

        builder.Property(p => p.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(p => p.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");
    }
}
