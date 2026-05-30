using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.UploadLinks;

public record ListFollowerUploadLinksQuery(
    Guid ProjectId,
    Guid FollowerId,
    Guid OwnerId) : IRequest<Result<List<FollowerUploadLinkDto>>>;

public class ListFollowerUploadLinksQueryHandler
    : IRequestHandler<ListFollowerUploadLinksQuery, Result<List<FollowerUploadLinkDto>>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFollowerRepository _followerRepository;
    private readonly IFollowerUploadLinkRepository _linkRepository;

    public ListFollowerUploadLinksQueryHandler(
        IProjectRepository projectRepository,
        IProjectFollowerRepository followerRepository,
        IFollowerUploadLinkRepository linkRepository)
    {
        _projectRepository = projectRepository;
        _followerRepository = followerRepository;
        _linkRepository = linkRepository;
    }

    public async Task<Result<List<FollowerUploadLinkDto>>> Handle(
        ListFollowerUploadLinksQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<List<FollowerUploadLinkDto>>.Failure("PROJECT_NOT_FOUND", "Project not found");

        var follower = await _followerRepository.GetByIdForOwnerAsync(
            request.FollowerId, request.OwnerId, cancellationToken);
        if (follower is null || follower.ProjectId != request.ProjectId)
            return Result<List<FollowerUploadLinkDto>>.Failure("FOLLOWER_NOT_FOUND", "Follower not found");

        var links = await _linkRepository.GetByFollowerForOwnerAsync(
            request.FollowerId, request.OwnerId, cancellationToken);

        return Result<List<FollowerUploadLinkDto>>.Success(
            links.Select(l => new FollowerUploadLinkDto(
                l.Id, l.ProjectId, l.FollowerId, l.TokenPreview,
                l.ExpiresAtUtc, l.IsActive, l.IsRevoked,
                l.CreatedAtUtc, l.LastUsedAtUtc)).ToList());
    }
}
