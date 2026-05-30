using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Akar.Infrastructure.Persistence.Configurations;

public class ProjectFolderConfiguration : IEntityTypeConfiguration<ProjectFolder>
{
    public void Configure(EntityTypeBuilder<ProjectFolder> builder)
    {
        builder.ToTable("project_folders");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(f => f.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(f => f.ParentFolderId)
            .HasColumnName("parent_folder_id");

        builder.Property(f => f.FolderName)
            .HasColumnName("folder_name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(f => f.FolderType)
            .HasColumnName("folder_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.IsSystemFolder)
            .HasColumnName("is_system_folder")
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
        builder.HasIndex(f => new { f.ProjectId, f.FolderType });

        // Relationships
        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Owner)
            .WithMany()
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.ParentFolder)
            .WithMany()
            .HasForeignKey(f => f.ParentFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore the Files navigation collection mapping (it's mapped from ProjectFile side)
        builder.Ignore(f => f.Files);
    }
}
