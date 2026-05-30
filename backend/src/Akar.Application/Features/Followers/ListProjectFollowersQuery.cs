using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Followers;

public record ListProjectFollowersQuery(
    Guid ProjectId,
    Guid OwnerId) : IRequest<Result<List<ProjectFollowerDto>>>;

public class ListProjectFollowersQueryHandler
    : IRequestHandler<ListProjectFollowersQuery, Result<List<ProjectFollowerDto>>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFollowerRepository _followerRepository;

    public ListProjectFollowersQueryHandler(
        IProjectRepository projectRepository,
        IProjectFollowerRepository followerRepository)
    {
        _projectRepository = projectRepository;
        _followerRepository = followerRepository;
    }

    public async Task<Result<List<ProjectFollowerDto>>> Handle(
        ListProjectFollowersQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);

        if (project is null)
            return Result<List<ProjectFollowerDto>>.Failure("PROJECT_NOT_FOUND", "Project not found");

        var followers = await _followerRepository.GetActiveByProjectForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);

        return Result<List<ProjectFollowerDto>>.Success(
            followers.Select(MapToDto).ToList());
    }

    internal static ProjectFollowerDto MapToDto(Domain.Entities.ProjectFollower f) => new(
        f.Id, f.ProjectId, f.OwnerId, f.InboxFolderId,
        f.FullName, f.Phone, f.FollowerType.ToString(),
        f.Notes, f.IsActive, f.IsDeleted,
        f.CreatedAtUtc, f.UpdatedAtUtc);
}
