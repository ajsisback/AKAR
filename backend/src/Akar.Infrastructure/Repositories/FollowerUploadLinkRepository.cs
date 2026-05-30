using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class FollowerUploadLinkRepository : IFollowerUploadLinkRepository
{
    private readonly AkarDbContext _context;

    public FollowerUploadLinkRepository(AkarDbContext context) => _context = context;

    public async Task<FollowerUploadLink?> GetByIdForOwnerAsync(Guid linkId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.FollowerUploadLinks
            .FirstOrDefaultAsync(l => l.Id == linkId && l.OwnerId == ownerId, cancellationToken);

    public async Task<FollowerUploadLink?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => await _context.FollowerUploadLinks
            .FirstOrDefaultAsync(l => l.TokenHash == tokenHash, cancellationToken);

    public async Task<List<FollowerUploadLink>> GetByFollowerForOwnerAsync(Guid followerId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.FollowerUploadLinks
            .Where(l => l.FollowerId == followerId && l.OwnerId == ownerId)
            .OrderByDescending(l => l.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(FollowerUploadLink link, CancellationToken cancellationToken = default)
        => await _context.FollowerUploadLinks.AddAsync(link, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
