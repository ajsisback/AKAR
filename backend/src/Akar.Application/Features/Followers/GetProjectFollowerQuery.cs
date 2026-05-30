using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Followers;

public record GetProjectFollowerQuery(
    Guid ProjectId,
    Guid FollowerId,
    Guid OwnerId) : IRequest<Result<ProjectFollowerDto>>;

public class GetProjectFollowerQueryHandler
    : IRequestHandler<GetProjectFollowerQuery, Result<ProjectFollowerDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFollowerRepository _followerRepository;

    public GetProjectFollowerQueryHandler(
        IProjectRepository projectRepository,
        IProjectFollowerRepository followerRepository)
    {
        _projectRepository = projectRepository;
        _followerRepository = followerRepository;
    }

    public async Task<Result<ProjectFollowerDto>> Handle(
        GetProjectFollowerQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);

        if (project is null)
            return Result<ProjectFollowerDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        var follower = await _followerRepository.GetByIdForOwnerAsync(
            request.FollowerId, request.OwnerId, cancellationToken);

        if (follower is null || follower.ProjectId != request.ProjectId)
            return Result<ProjectFollowerDto>.Failure("FOLLOWER_NOT_FOUND", "Follower not found");

        return Result<ProjectFollowerDto>.Success(
            ListProjectFollowersQueryHandler.MapToDto(follower));
    }
}
