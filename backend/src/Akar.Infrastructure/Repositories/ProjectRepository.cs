using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AkarDbContext _context;

    public ProjectRepository(AkarDbContext context) => _context = context;

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Project?> GetByIdForOwnerAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerId, cancellationToken);

    public async Task<List<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.Projects.Where(p => p.OwnerId == ownerId).OrderByDescending(p => p.CreatedAtUtc).ToListAsync(cancellationToken);

    public async Task<int> CountByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.Projects.CountAsync(p => p.OwnerId == ownerId, cancellationToken);

    public async Task<int> CountByOwnerIdAndStageAsync(Guid ownerId, CurrentStage stage, CancellationToken cancellationToken = default)
        => await _context.Projects.CountAsync(p => p.OwnerId == ownerId && p.CurrentStage == stage, cancellationToken);

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
        => await _context.Projects.AddAsync(project, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
