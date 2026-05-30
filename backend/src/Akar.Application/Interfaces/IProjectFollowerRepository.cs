using Akar.Domain.Entities;

namespace Akar.Application.Interfaces;

public interface IProjectFollowerRepository
{
    Task<ProjectFollower?> GetByIdForOwnerAsync(Guid followerId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<List<ProjectFollower>> GetActiveByProjectForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPhoneInProjectAsync(Guid projectId, string phone, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPhoneInProjectExcludingAsync(Guid projectId, string phone, Guid excludeFollowerId, CancellationToken cancellationToken = default);
    Task AddAsync(ProjectFollower follower, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
