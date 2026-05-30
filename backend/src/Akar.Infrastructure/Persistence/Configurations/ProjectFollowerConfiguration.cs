using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Akar.Infrastructure.Persistence.Configurations;

public class ProjectFollowerConfiguration : IEntityTypeConfiguration<ProjectFollower>
{
    public void Configure(EntityTypeBuilder<ProjectFollower> builder)
    {
        builder.ToTable("project_followers");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(f => f.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(f => f.InboxFolderId)
            .HasColumnName("inbox_folder_id")
            .IsRequired();

        builder.Property(f => f.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.Phone)
            .HasColumnName("phone")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(f => f.FollowerType)
            .HasColumnName("follower_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(f => f.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(f => f.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(f => f.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(f => f.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(f => f.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // Indexes
        builder.HasIndex(f => f.ProjectId);
        builder.HasIndex(f => f.OwnerId);
        builder.HasIndex(f => f.InboxFolderId);
        builder.HasIndex(f => new { f.ProjectId, f.Phone });
        builder.HasIndex(f => f.IsDeleted);

        // Relationships
        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Owner)
            .WithMany()
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.InboxFolder)
            .WithMany()
            .HasForeignKey(f => f.InboxFolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
