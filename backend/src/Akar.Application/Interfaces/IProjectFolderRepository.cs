using Akar.Domain.Entities;
using Akar.Domain.Enums;

namespace Akar.Application.Interfaces;

public interface IProjectFolderRepository
{
    Task<ProjectFolder?> GetByIdForOwnerAsync(Guid folderId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<List<ProjectFolder>> GetByProjectIdForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> HasSystemFoldersAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task AddAsync(ProjectFolder folder, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<ProjectFolder> folders, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
