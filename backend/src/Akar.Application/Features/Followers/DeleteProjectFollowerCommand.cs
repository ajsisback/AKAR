using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Followers;

public record DeleteProjectFollowerCommand(
    Guid ProjectId,
    Guid FollowerId,
    Guid OwnerId) : IRequest<Result>;

public class DeleteProjectFollowerCommandHandler
    : IRequestHandler<DeleteProjectFollowerCommand, Result>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFollowerRepository _followerRepository;

    public DeleteProjectFollowerCommandHandler(
        IProjectRepository projectRepository,
        IProjectFollowerRepository followerRepository)
    {
        _projectRepository = projectRepository;
        _followerRepository = followerRepository;
    }

    public async Task<Result> Handle(
        DeleteProjectFollowerCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);

        if (project is null)
            return Result.Failure("PROJECT_NOT_FOUND", "Project not found");

        var follower = await _followerRepository.GetByIdForOwnerAsync(
            request.FollowerId, request.OwnerId, cancellationToken);

        if (follower is null || follower.ProjectId != request.ProjectId)
            return Result.Failure("FOLLOWER_NOT_FOUND", "Follower not found");

        // Soft-delete follower only. Inbox folder remains active for audit/history.
        // Physical files are preserved.
        if (!follower.SoftDelete())
            return Result.Failure("FOLLOWER_NOT_FOUND", "Follower is already deleted");

        await _followerRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
