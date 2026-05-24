using Akar.Domain.Common;
using Akar.Domain.Enums;

namespace Akar.Domain.Entities;

/// <summary>
/// A folder within a project's document vault.
/// System folders are auto-created; custom folders are created by the owner.
/// </summary>
public class ProjectFolder : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid? ParentFolderId { get; private set; }
    public string FolderName { get; private set; } = string.Empty;
    public FolderType FolderType { get; private set; }
    public bool IsSystemFolder { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    // Navigation
    public Project Project { get; private set; } = null!;
    public Owner Owner { get; private set; } = null!;
    public ProjectFolder? ParentFolder { get; private set; }
    public IReadOnlyCollection<ProjectFile> Files => _files.AsReadOnly();
    private readonly List<ProjectFile> _files = [];

    private ProjectFolder() { } // EF Core

    /// <summary>Creates a system folder (auto-generated per project).</summary>
    public static ProjectFolder CreateSystemFolder(
        Guid projectId,
        Guid ownerId,
        FolderType folderType,
        string folderName)
    {
        return new ProjectFolder(Guid.NewGuid())
        {
            ProjectId = projectId,
            OwnerId = ownerId,
            FolderType = folderType,
            FolderName = folderName,
            IsSystemFolder = true,
            IsDeleted = false
        };
    }

    /// <summary>Creates a custom folder (user-created).</summary>
    public static ProjectFolder CreateCustomFolder(
        Guid projectId,
        Guid ownerId,
        string folderName,
        Guid? parentFolderId = null)
    {
        return new ProjectFolder(Guid.NewGuid())
        {
            ProjectId = projectId,
            OwnerId = ownerId,
            FolderType = FolderType.Custom,
            FolderName = folderName,
            ParentFolderId = parentFolderId,
            IsSystemFolder = false,
            IsDeleted = false
        };
    }

    /// <summary>Renames the folder. Only custom folders can be renamed.</summary>
    public bool Rename(string newName)
    {
        if (IsSystemFolder) return false;
        FolderName = newName;
        SetUpdatedAt();
        return true;
    }

    /// <summary>Soft-deletes the folder. System folders and Trash cannot be deleted.</summary>
    public bool SoftDelete()
    {
        if (IsSystemFolder) return false;
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        SetUpdatedAt();
        return true;
    }

    private ProjectFolder(Guid id) : base(id) { }

    /// <summary>
    /// Default system folder types and their display names.
    /// </summary>
    public static IReadOnlyList<(FolderType Type, string Name)> DefaultSystemFolders { get; } =
    [
        (FolderType.License, "License"),
        (FolderType.ProjectLocation, "Project Location"),
        (FolderType.Contracts, "Contracts"),
        (FolderType.Drawings, "Drawings"),
        (FolderType.Photos, "Photos"),
        (FolderType.Videos, "Videos"),
        (FolderType.Invoices, "Invoices"),
        (FolderType.Warranties, "Warranties"),
        (FolderType.FollowersInbox, "Followers Inbox"),
        (FolderType.Trash, "Trash"),
    ];
}
