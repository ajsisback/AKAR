using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Application.Services;
using Akar.Domain.Entities;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.UploadLinks;

public record GenerateFollowerUploadLinkCommand(
    Guid ProjectId,
    Guid FollowerId,
    Guid OwnerId,
    DateTime? ExpiresAtUtc) : IRequest<Result<GenerateFollowerUploadLinkResponseDto>>;

public class GenerateFollowerUploadLinkCommandHandler
    : IRequestHandler<GenerateFollowerUploadLinkCommand, Result<GenerateFollowerUploadLinkResponseDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFollowerRepository _followerRepository;
    private readonly IFollowerUploadLinkRepository _linkRepository;

    public GenerateFollowerUploadLinkCommandHandler(
        IProjectRepository projectRepository,
        IProjectFollowerRepository followerRepository,
        IFollowerUploadLinkRepository linkRepository)
    {
        _projectRepository = projectRepository;
        _followerRepository = followerRepository;
        _linkRepository = linkRepository;
    }

    public async Task<Result<GenerateFollowerUploadLinkResponseDto>> Handle(
        GenerateFollowerUploadLinkCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<GenerateFollowerUploadLinkResponseDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        var follower = await _followerRepository.GetByIdForOwnerAsync(
            request.FollowerId, request.OwnerId, cancellationToken);
        if (follower is null || follower.ProjectId != request.ProjectId)
            return Result<GenerateFollowerUploadLinkResponseDto>.Failure("FOLLOWER_NOT_FOUND", "Follower not found");

        if (!follower.IsActive || follower.IsDeleted)
            return Result<GenerateFollowerUploadLinkResponseDto>.Failure("FOLLOWER_INACTIVE", "Follower is inactive or deleted");

        // Generate token
        var rawToken = UploadTokenService.GenerateToken();
        var tokenHash = UploadTokenService.HashToken(rawToken);
        var tokenPreview = UploadTokenService.GetPreview(rawToken);

        var link = FollowerUploadLink.Create(
            request.ProjectId, request.OwnerId, request.FollowerId,
            tokenHash, tokenPreview, request.ExpiresAtUtc);

        await _linkRepository.AddAsync(link, cancellationToken);
        await _linkRepository.SaveChangesAsync(cancellationToken);

        var uploadUrl = $"/api/public/follower-upload/{rawToken}/files";

        return Result<GenerateFollowerUploadLinkResponseDto>.Success(
            new GenerateFollowerUploadLinkResponseDto(
                link.Id, rawToken, uploadUrl, link.ExpiresAtUtc));
    }
}
