using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class ProjectContractRepository : IProjectContractRepository
{
    private readonly AkarDbContext _db;

    public ProjectContractRepository(AkarDbContext db) => _db = db;

    public async Task<List<ProjectContract>> GetActiveByProjectForOwnerAsync(
        Guid projectId, Guid ownerId, CancellationToken cancellationToken = default) =>
        await _db.ProjectContracts
            .Include(c => c.ContractTemplate)
            .Where(c => c.ProjectId == projectId && c.OwnerId == ownerId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<ProjectContract?> GetByIdForOwnerAsync(
        Guid contractId, Guid ownerId, CancellationToken cancellationToken = default) =>
        await _db.ProjectContracts
            .Include(c => c.ContractTemplate)
            .FirstOrDefaultAsync(c => c.Id == contractId && c.OwnerId == ownerId && !c.IsDeleted, cancellationToken);

    public async Task AddAsync(ProjectContract contract, CancellationToken cancellationToken = default) =>
        await _db.ProjectContracts.AddAsync(contract, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _db.SaveChangesAsync(cancellationToken);
}
