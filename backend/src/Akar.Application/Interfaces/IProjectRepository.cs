using Akar.Domain.Entities;

namespace Akar.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdForOwnerAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
    Task<List<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<int> CountByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<int> CountByOwnerIdAndStageAsync(Guid ownerId, Domain.Enums.CurrentStage stage, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
