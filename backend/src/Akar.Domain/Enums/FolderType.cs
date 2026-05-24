namespace Akar.Domain.Enums;

/// <summary>
/// Types of folders within a project document vault.
/// System folders are created automatically; Custom folders are user-created.
/// </summary>
public enum FolderType
{
    License = 0,
    ProjectLocation = 1,
    Contracts = 2,
    Drawings = 3,
    Photos = 4,
    Videos = 5,
    Invoices = 6,
    Warranties = 7,
    FollowersInbox = 8,
    Trash = 9,
    Custom = 10
}
