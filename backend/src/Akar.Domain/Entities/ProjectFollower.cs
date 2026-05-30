using Akar.Domain.Common;
using Akar.Domain.Enums;

namespace Akar.Domain.Entities;

/// <summary>
/// A follower/helper attached to a project by the owner.
/// Each follower has a dedicated inbox folder under the project's FollowersInbox.
/// In Sprint 3A the follower has no login — they are a record only.
/// </summary>
public class ProjectFollower : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid InboxFolderId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public FollowerType FollowerType { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    // Navigation
    public Project Project { get; private set; } = null!;
    public Owner Owner { get; private set; } = null!;
    public ProjectFolder InboxFolder { get; private set; } = null!;

    private ProjectFollower() { } // EF Core

    public static ProjectFollower Create(
        Guid projectId,
        Guid ownerId,
        Guid inboxFolderId,
        string fullName,
        string phone,
        FollowerType followerType,
        string? notes)
    {
        return new ProjectFollower(Guid.NewGuid())
        {
            ProjectId = projectId,
            OwnerId = ownerId,
            InboxFolderId = inboxFolderId,
            FullName = fullName.Trim(),
            Phone = phone.Trim(),
            FollowerType = followerType,
            Notes = notes?.Trim(),
            IsActive = true,
            IsDeleted = false
        };
    }

    /// <summary>Updates follower details. Returns false if follower is deleted.</summary>
    public bool Update(string fullName, string phone, FollowerType followerType, string? notes, bool isActive)
    {
        if (IsDeleted) return false;
        FullName = fullName.Trim();
        Phone = phone.Trim();
        FollowerType = followerType;
        Notes = notes?.Trim();
        IsActive = isActive;
        SetUpdatedAt();
        return true;
    }

    /// <summary>Soft-deletes the follower. Inbox folder is preserved for audit/history.</summary>
    public bool SoftDelete()
    {
        if (IsDeleted) return false;
        IsDeleted = true;
        IsActive = false;
        DeletedAtUtc = DateTime.UtcNow;
        SetUpdatedAt();
        return true;
    }

    private ProjectFollower(Guid id) : base(id) { }
}
