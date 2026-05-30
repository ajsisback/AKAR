using Akar.Domain.Common;
using Akar.Domain.Enums;

namespace Akar.Domain.Entities;

/// <summary>
/// Represents a timeline event inside a project.
/// Tracks stage changes, manual notes, and future system-generated events.
/// </summary>
public class ProjectTimelineEvent : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid OwnerId { get; private set; }
    public TimelineEventType EventType { get; private set; }
    public CurrentStage Stage { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime EventDateUtc { get; private set; }
    public TimelineSourceType SourceType { get; private set; }
    public Guid? SourceId { get; private set; }
    public bool IsSystemGenerated { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    // Navigation
    public Project Project { get; private set; } = null!;
    public Owner Owner { get; private set; } = null!;

    private ProjectTimelineEvent() { } // EF Core
    private ProjectTimelineEvent(Guid id) : base(id) { }

    /// <summary>
    /// Creates a manual timeline note.
    /// </summary>
    public static ProjectTimelineEvent CreateManualNote(
        Guid projectId,
        Guid ownerId,
        CurrentStage stage,
        string title,
        string? description,
        DateTime? eventDateUtc = null)
    {
        return new ProjectTimelineEvent(Guid.NewGuid())
        {
            ProjectId = projectId,
            OwnerId = ownerId,
            EventType = TimelineEventType.ManualNote,
            Stage = stage,
            Title = title,
            Description = description,
            EventDateUtc = eventDateUtc ?? DateTime.UtcNow,
            SourceType = TimelineSourceType.None,
            SourceId = null,
            IsSystemGenerated = false,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Creates a system-generated stage changed event.
    /// </summary>
    public static ProjectTimelineEvent CreateStageChanged(
        Guid projectId,
        Guid ownerId,
        CurrentStage newStage,
        string? note = null)
    {
        return new ProjectTimelineEvent(Guid.NewGuid())
        {
            ProjectId = projectId,
            OwnerId = ownerId,
            EventType = TimelineEventType.StageChanged,
            Stage = newStage,
            Title = $"Stage changed to {newStage}",
            Description = note,
            EventDateUtc = DateTime.UtcNow,
            SourceType = TimelineSourceType.Project,
            SourceId = projectId,
            IsSystemGenerated = true,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Soft-deletes the event. Only manual events can be deleted.
    /// Returns false if the event is system-generated.
    /// </summary>
    public bool SoftDelete()
    {
        if (IsSystemGenerated) return false;
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        SetUpdatedAt();
        return true;
    }
}
