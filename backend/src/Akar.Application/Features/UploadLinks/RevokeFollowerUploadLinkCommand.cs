using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.UploadLinks;

public record RevokeFollowerUploadLinkCommand(
    Guid ProjectId,
    Guid FollowerId,
    Guid LinkId,
    Guid OwnerId) : IRequest<Result>;

public class RevokeFollowerUploadLinkCommandHandler
    : IRequestHandler<RevokeFollowerUploadLinkCommand, Result>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFollowerRepository _followerRepository;
    private readonly IFollowerUploadLinkRepository _linkRepository;

    public RevokeFollowerUploadLinkCommandHandler(
        IProjectRepository projectRepository,
        IProjectFollowerRepository followerRepository,
        IFollowerUploadLinkRepository linkRepository)
    {
        _projectRepository = projectRepository;
        _followerRepository = followerRepository;
        _linkRepository = linkRepository;
    }

    public async Task<Result> Handle(
        RevokeFollowerUploadLinkCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result.Failure("PROJECT_NOT_FOUND", "Project not found");

        var follower = await _followerRepository.GetByIdForOwnerAsync(
            request.FollowerId, request.OwnerId, cancellationToken);
        if (follower is null || follower.ProjectId != request.ProjectId)
            return Result.Failure("FOLLOWER_NOT_FOUND", "Follower not found");

        var link = await _linkRepository.GetByIdForOwnerAsync(
            request.LinkId, request.OwnerId, cancellationToken);
        if (link is null || link.FollowerId != request.FollowerId)
            return Result.Failure("UPLOAD_LINK_NOT_FOUND", "Upload link not found");

        if (!link.Revoke())
            return Result.Failure("UPLOAD_LINK_NOT_FOUND", "Upload link is already revoked");

        await _linkRepository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
