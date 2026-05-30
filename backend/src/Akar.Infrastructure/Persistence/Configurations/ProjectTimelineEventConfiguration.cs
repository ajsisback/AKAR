using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Akar.Infrastructure.Persistence.Configurations;

public class ProjectTimelineEventConfiguration : IEntityTypeConfiguration<ProjectTimelineEvent>
{
    public void Configure(EntityTypeBuilder<ProjectTimelineEvent> builder)
    {
        builder.ToTable("project_timeline_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(e => e.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Stage)
            .HasColumnName("stage")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(e => e.EventDateUtc)
            .HasColumnName("event_date_utc")
            .IsRequired();

        builder.Property(e => e.SourceType)
            .HasColumnName("source_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.SourceId)
            .HasColumnName("source_id");

        builder.Property(e => e.IsSystemGenerated)
            .HasColumnName("is_system_generated")
            .IsRequired();

        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.Property(e => e.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // Relationships
        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => new { e.ProjectId, e.EventDateUtc });
        builder.HasIndex(e => new { e.ProjectId, e.Stage });
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.SourceType);
        builder.HasIndex(e => e.IsDeleted);
    }
}
