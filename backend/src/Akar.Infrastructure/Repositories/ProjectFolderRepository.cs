using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class ProjectFolderRepository : IProjectFolderRepository
{
    private readonly AkarDbContext _context;

    public ProjectFolderRepository(AkarDbContext context) => _context = context;

    public async Task<ProjectFolder?> GetByIdForOwnerAsync(Guid folderId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFolders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.OwnerId == ownerId && !f.IsDeleted, cancellationToken);

    public async Task<List<ProjectFolder>> GetByProjectIdForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFolders
            .Where(f => f.ProjectId == projectId && f.OwnerId == ownerId && !f.IsDeleted)
            .OrderBy(f => f.FolderType)
            .ThenBy(f => f.FolderName)
            .ToListAsync(cancellationToken);

    public async Task<List<ProjectFolder>> GetDeletedByProjectForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFolders
            .Where(f => f.ProjectId == projectId && f.OwnerId == ownerId && f.IsDeleted)
            .OrderByDescending(f => f.DeletedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasSystemFoldersAsync(Guid projectId, CancellationToken cancellationToken = default)
        => await _context.ProjectFolders
            .AnyAsync(f => f.ProjectId == projectId && f.IsSystemFolder, cancellationToken);

    public async Task<ProjectFolder?> GetSystemFolderAsync(Guid projectId, FolderType folderType, CancellationToken cancellationToken = default)
        => await _context.ProjectFolders
            .FirstOrDefaultAsync(f => f.ProjectId == projectId && f.FolderType == folderType && f.IsSystemFolder && !f.IsDeleted, cancellationToken);

    public async Task AddAsync(ProjectFolder folder, CancellationToken cancellationToken = default)
        => await _context.ProjectFolders.AddAsync(folder, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<ProjectFolder> folders, CancellationToken cancellationToken = default)
        => await _context.ProjectFolders.AddRangeAsync(folders, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
