using Akar.Domain.Entities;
using Akar.Domain.Enums;

namespace Akar.Application.Interfaces;

public interface IProjectTimelineRepository
{
    Task<List<ProjectTimelineEvent>> GetActiveByProjectForOwnerAsync(
        Guid projectId, Guid ownerId,
        CurrentStage? stage = null,
        TimelineEventType? eventType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<ProjectTimelineEvent?> GetByIdForOwnerAsync(
        Guid eventId, Guid ownerId, CancellationToken cancellationToken = default);

    Task<DateTime?> GetLastStageChangeAsync(
        Guid projectId, Guid ownerId, CancellationToken cancellationToken = default);

    Task AddAsync(ProjectTimelineEvent timelineEvent, CancellationToken cancellationToken = default);

    Task<bool> ExistsForSourceAsync(
        Guid projectId, Guid ownerId,
        TimelineEventType eventType, TimelineSourceType sourceType, Guid sourceId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
