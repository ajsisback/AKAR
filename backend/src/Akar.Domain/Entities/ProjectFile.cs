using Akar.Domain.Common;
using Akar.Domain.Enums;

namespace Akar.Domain.Entities;

/// <summary>
/// File metadata within a project folder.
/// Physical file storage is handled by IFileStorageService (not implemented in Sprint 2A).
/// </summary>
public class ProjectFile : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid FolderId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string FileExtension { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public StorageProvider StorageProvider { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public FileCategory FileCategory { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    // Navigation
    public Project Project { get; private set; } = null!;
    public Owner Owner { get; private set; } = null!;
    public ProjectFolder Folder { get; private set; } = null!;

    private ProjectFile() { } // EF Core

    public static ProjectFile Create(
        Guid projectId,
        Guid ownerId,
        Guid folderId,
        string originalFileName,
        string storedFileName,
        string contentType,
        string fileExtension,
        long fileSizeBytes,
        StorageProvider storageProvider,
        string storagePath,
        FileCategory fileCategory)
    {
        return new ProjectFile(Guid.NewGuid())
        {
            ProjectId = projectId,
            OwnerId = ownerId,
            FolderId = folderId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = contentType,
            FileExtension = fileExtension,
            FileSizeBytes = fileSizeBytes,
            StorageProvider = storageProvider,
            StoragePath = storagePath,
            FileCategory = fileCategory,
            IsDeleted = false
        };
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        SetUpdatedAt();
    }

    private ProjectFile(Guid id) : base(id) { }
}
