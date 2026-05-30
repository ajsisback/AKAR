using Akar.Domain.Entities;

namespace Akar.Application.Interfaces;

public interface IProjectContractRepository
{
    Task<List<ProjectContract>> GetActiveByProjectForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<ProjectContract?> GetByIdForOwnerAsync(Guid contractId, Guid ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(ProjectContract contract, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
