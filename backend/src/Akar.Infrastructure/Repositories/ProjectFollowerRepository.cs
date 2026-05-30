using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class ProjectFollowerRepository : IProjectFollowerRepository
{
    private readonly AkarDbContext _context;

    public ProjectFollowerRepository(AkarDbContext context) => _context = context;

    public async Task<ProjectFollower?> GetByIdForOwnerAsync(Guid followerId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFollowers
            .FirstOrDefaultAsync(f => f.Id == followerId && f.OwnerId == ownerId && !f.IsDeleted, cancellationToken);

    public async Task<List<ProjectFollower>> GetActiveByProjectForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFollowers
            .Where(f => f.ProjectId == projectId && f.OwnerId == ownerId && !f.IsDeleted)
            .OrderBy(f => f.FullName)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByPhoneInProjectAsync(Guid projectId, string phone, CancellationToken cancellationToken = default)
        => await _context.ProjectFollowers
            .AnyAsync(f => f.ProjectId == projectId && f.Phone == phone && !f.IsDeleted, cancellationToken);

    public async Task<bool> ExistsByPhoneInProjectExcludingAsync(Guid projectId, string phone, Guid excludeFollowerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFollowers
            .AnyAsync(f => f.ProjectId == projectId && f.Phone == phone && f.Id != excludeFollowerId && !f.IsDeleted, cancellationToken);

    public async Task AddAsync(ProjectFollower follower, CancellationToken cancellationToken = default)
        => await _context.ProjectFollowers.AddAsync(follower, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
