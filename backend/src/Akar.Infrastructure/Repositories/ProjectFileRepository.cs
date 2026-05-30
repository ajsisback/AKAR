using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class ProjectFileRepository : IProjectFileRepository
{
    private readonly AkarDbContext _context;

    public ProjectFileRepository(AkarDbContext context) => _context = context;

    public async Task<ProjectFile?> GetByIdForOwnerAsync(Guid fileId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.OwnerId == ownerId, cancellationToken);

    public async Task<List<ProjectFile>> GetActiveByFolderForOwnerAsync(Guid folderId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles
            .Where(f => f.FolderId == folderId && f.OwnerId == ownerId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<List<ProjectFile>> GetDeletedByProjectForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles
            .Where(f => f.ProjectId == projectId && f.OwnerId == ownerId && f.IsDeleted)
            .OrderByDescending(f => f.DeletedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<int> CountActiveByFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles
            .CountAsync(f => f.FolderId == folderId && !f.IsDeleted, cancellationToken);

    public async Task AddAsync(ProjectFile file, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles.AddAsync(file, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
