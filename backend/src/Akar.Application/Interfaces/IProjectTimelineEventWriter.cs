using Akar.Domain.Enums;

namespace Akar.Application.Interfaces;

/// <summary>
/// Application service for creating automatic timeline events consistently.
/// </summary>
public interface IProjectTimelineEventWriter
{
    /// <summary>
    /// Creates a system-generated timeline event with duplicate prevention.
    /// If an event with the same source already exists, it is silently skipped.
    /// </summary>
    Task AddSystemEventAsync(
        Guid projectId,
        Guid ownerId,
        CurrentStage stage,
        TimelineEventType eventType,
        TimelineSourceType sourceType,
        Guid sourceId,
        string title,
        string? description,
        CancellationToken cancellationToken = default);
}
