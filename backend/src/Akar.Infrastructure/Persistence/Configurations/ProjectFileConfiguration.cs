using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Akar.Infrastructure.Persistence.Configurations;

public class ProjectFileConfiguration : IEntityTypeConfiguration<ProjectFile>
{
    public void Configure(EntityTypeBuilder<ProjectFile> builder)
    {
        builder.ToTable("project_files");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(f => f.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(f => f.FolderId)
            .HasColumnName("folder_id")
            .IsRequired();

        builder.Property(f => f.OriginalFileName)
            .HasColumnName("original_file_name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.StoredFileName)
            .HasColumnName("stored_file_name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.FileExtension)
            .HasColumnName("file_extension")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.FileSizeBytes)
            .HasColumnName("file_size_bytes")
            .IsRequired();

        builder.Property(f => f.StorageProvider)
            .HasColumnName("storage_provider")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.StoragePath)
            .HasColumnName("storage_path")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(f => f.FileCategory)
            .HasColumnName("file_category")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(f => f.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        builder.Property(f => f.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(f => f.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        // Indexes
        builder.HasIndex(f => f.ProjectId);
        builder.HasIndex(f => f.OwnerId);
        builder.HasIndex(f => f.FolderId);
        builder.HasIndex(f => new { f.ProjectId, f.FolderId });
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

        builder.HasOne(f => f.Folder)
            .WithMany()
            .HasForeignKey(f => f.FolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
