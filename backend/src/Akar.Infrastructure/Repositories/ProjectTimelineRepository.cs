using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class ProjectTimelineRepository : IProjectTimelineRepository
{
    private readonly AkarDbContext _db;

    public ProjectTimelineRepository(AkarDbContext db) => _db = db;

    public async Task<List<ProjectTimelineEvent>> GetActiveByProjectForOwnerAsync(
        Guid projectId, Guid ownerId,
        CurrentStage? stage = null,
        TimelineEventType? eventType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ProjectTimelineEvents
            .Where(e => e.ProjectId == projectId && e.OwnerId == ownerId && !e.IsDeleted);

        if (stage.HasValue)
            query = query.Where(e => e.Stage == stage.Value);

        if (eventType.HasValue)
            query = query.Where(e => e.EventType == eventType.Value);

        if (fromDate.HasValue)
            query = query.Where(e => e.EventDateUtc >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.EventDateUtc <= toDate.Value);

        return await query
            .OrderByDescending(e => e.EventDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectTimelineEvent?> GetByIdForOwnerAsync(
        Guid eventId, Guid ownerId, CancellationToken cancellationToken = default) =>
        await _db.ProjectTimelineEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.OwnerId == ownerId && !e.IsDeleted, cancellationToken);

    public async Task<DateTime?> GetLastStageChangeAsync(
        Guid projectId, Guid ownerId, CancellationToken cancellationToken = default) =>
        await _db.ProjectTimelineEvents
            .Where(e => e.ProjectId == projectId && e.OwnerId == ownerId
                     && e.EventType == TimelineEventType.StageChanged && !e.IsDeleted)
            .OrderByDescending(e => e.EventDateUtc)
            .Select(e => (DateTime?)e.EventDateUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(ProjectTimelineEvent timelineEvent, CancellationToken cancellationToken = default) =>
        await _db.ProjectTimelineEvents.AddAsync(timelineEvent, cancellationToken);

    public async Task<bool> ExistsForSourceAsync(
        Guid projectId, Guid ownerId,
        TimelineEventType eventType, TimelineSourceType sourceType, Guid sourceId,
        CancellationToken cancellationToken = default) =>
        await _db.ProjectTimelineEvents.AnyAsync(
            e => e.ProjectId == projectId && e.OwnerId == ownerId
              && e.EventType == eventType && e.SourceType == sourceType
              && e.SourceId == sourceId && !e.IsDeleted,
            cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _db.SaveChangesAsync(cancellationToken);
}
