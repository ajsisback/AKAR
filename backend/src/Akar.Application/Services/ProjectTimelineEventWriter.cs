using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;

namespace Akar.Application.Services;

/// <summary>
/// Creates automatic timeline events with duplicate prevention.
/// Used by command handlers to consistently track project activity.
/// </summary>
public class ProjectTimelineEventWriter : IProjectTimelineEventWriter
{
    private readonly IProjectTimelineRepository _repo;

    public ProjectTimelineEventWriter(IProjectTimelineRepository repo) => _repo = repo;

    public async Task AddSystemEventAsync(
        Guid projectId,
        Guid ownerId,
        CurrentStage stage,
        TimelineEventType eventType,
        TimelineSourceType sourceType,
        Guid sourceId,
        string title,
        string? description,
        CancellationToken cancellationToken = default)
    {
        // Duplicate prevention
        var exists = await _repo.ExistsForSourceAsync(
            projectId, ownerId, eventType, sourceType, sourceId, cancellationToken);

        if (exists) return;

        var timelineEvent = ProjectTimelineEvent.CreateSystemEvent(
            projectId, ownerId, stage, eventType, sourceType, sourceId, title, description);

        await _repo.AddAsync(timelineEvent, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }
}
