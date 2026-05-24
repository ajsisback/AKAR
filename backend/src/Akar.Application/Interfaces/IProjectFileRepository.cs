using Akar.Domain.Entities;

namespace Akar.Application.Interfaces;

public interface IProjectFileRepository
{
    Task<ProjectFile?> GetByIdForOwnerAsync(Guid fileId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<List<ProjectFile>> GetActiveByFolderForOwnerAsync(Guid folderId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<List<ProjectFile>> GetDeletedByProjectForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<int> CountActiveByFolderAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task AddAsync(ProjectFile file, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
