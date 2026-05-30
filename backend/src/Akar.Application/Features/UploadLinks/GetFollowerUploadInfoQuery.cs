using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Application.Services;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.UploadLinks;

public record GetFollowerUploadInfoQuery(
    string RawToken) : IRequest<Result<FollowerUploadInfoDto>>;

public class GetFollowerUploadInfoQueryHandler
    : IRequestHandler<GetFollowerUploadInfoQuery, Result<FollowerUploadInfoDto>>
{
    private readonly IFollowerUploadLinkRepository _linkRepository;
    private readonly IProjectFollowerRepository _followerRepository;
    private readonly IProjectRepository _projectRepository;

    public GetFollowerUploadInfoQueryHandler(
        IFollowerUploadLinkRepository linkRepository,
        IProjectFollowerRepository followerRepository,
        IProjectRepository projectRepository)
    {
        _linkRepository = linkRepository;
        _followerRepository = followerRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<FollowerUploadInfoDto>> Handle(
        GetFollowerUploadInfoQuery request, CancellationToken cancellationToken)
    {
        var tokenHash = UploadTokenService.HashToken(request.RawToken);
        var link = await _linkRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (link is null)
            return Result<FollowerUploadInfoDto>.Failure("UPLOAD_LINK_NOT_FOUND", "Upload link not found");

        if (link.IsRevoked)
            return Result<FollowerUploadInfoDto>.Failure("UPLOAD_LINK_REVOKED", "Upload link has been revoked");

        if (link.ExpiresAtUtc.HasValue && link.ExpiresAtUtc.Value < DateTime.UtcNow)
            return Result<FollowerUploadInfoDto>.Failure("UPLOAD_LINK_EXPIRED", "Upload link has expired");

        if (!link.IsActive)
            return Result<FollowerUploadInfoDto>.Failure("UPLOAD_LINK_NOT_FOUND", "Upload link is inactive");

        // Get follower (use owner-scoped to verify consistency)
        var follower = await _followerRepository.GetByIdForOwnerAsync(
            link.FollowerId, link.OwnerId, cancellationToken);
        if (follower is null)
            return Result<FollowerUploadInfoDto>.Failure("FOLLOWER_NOT_FOUND", "Follower not found");

        if (!follower.IsActive || follower.IsDeleted)
            return Result<FollowerUploadInfoDto>.Failure("FOLLOWER_INACTIVE", "Follower is inactive");

        var project = await _projectRepository.GetByIdAsync(link.ProjectId, cancellationToken);
        if (project is null)
            return Result<FollowerUploadInfoDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        return Result<FollowerUploadInfoDto>.Success(new FollowerUploadInfoDto(
            follower.FullName,
            project.ProjectName,
            FileValidationService.GetAllowedExtensionNames(),
            FileValidationService.GetMaxFileSizeBytes(),
            link.ExpiresAtUtc));
    }
}
