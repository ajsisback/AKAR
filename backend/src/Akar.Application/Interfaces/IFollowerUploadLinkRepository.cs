using Akar.Domain.Entities;

namespace Akar.Application.Interfaces;

public interface IFollowerUploadLinkRepository
{
    Task<FollowerUploadLink?> GetByIdForOwnerAsync(Guid linkId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<FollowerUploadLink?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<List<FollowerUploadLink>> GetByFollowerForOwnerAsync(Guid followerId, Guid ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(FollowerUploadLink link, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
