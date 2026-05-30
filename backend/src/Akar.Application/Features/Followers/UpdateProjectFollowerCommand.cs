using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Enums;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Followers;

public record UpdateProjectFollowerCommand(
    Guid ProjectId,
    Guid FollowerId,
    Guid OwnerId,
    string FullName,
    string Phone,
    string FollowerType,
    string? Notes,
    bool IsActive) : IRequest<Result<ProjectFollowerDto>>;

public class UpdateProjectFollowerCommandHandler
    : IRequestHandler<UpdateProjectFollowerCommand, Result<ProjectFollowerDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFollowerRepository _followerRepository;
    private readonly IProjectFolderRepository _folderRepository;

    public UpdateProjectFollowerCommandHandler(
        IProjectRepository projectRepository,
        IProjectFollowerRepository followerRepository,
        IProjectFolderRepository folderRepository)
    {
        _projectRepository = projectRepository;
        _followerRepository = followerRepository;
        _folderRepository = folderRepository;
    }

    public async Task<Result<ProjectFollowerDto>> Handle(
        UpdateProjectFollowerCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);

        if (project is null)
            return Result<ProjectFollowerDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        if (!Enum.TryParse<FollowerType>(request.FollowerType, ignoreCase: true, out var followerType))
            return Result<ProjectFollowerDto>.Failure("INVALID_FOLLOWER_TYPE", "Invalid follower type");

        var follower = await _followerRepository.GetByIdForOwnerAsync(
            request.FollowerId, request.OwnerId, cancellationToken);

        if (follower is null || follower.ProjectId != request.ProjectId)
            return Result<ProjectFollowerDto>.Failure("FOLLOWER_NOT_FOUND", "Follower not found");

        // Check phone uniqueness (exclude self)
        if (await _followerRepository.ExistsByPhoneInProjectExcludingAsync(
                request.ProjectId, request.Phone.Trim(), request.FollowerId, cancellationToken))
            return Result<ProjectFollowerDto>.Failure("FOLLOWER_PHONE_ALREADY_EXISTS",
                "A follower with this phone number already exists in the project");

        var oldName = follower.FullName;
        if (!follower.Update(request.FullName, request.Phone, followerType, request.Notes, request.IsActive))
            return Result<ProjectFollowerDto>.Failure("FOLLOWER_NOT_FOUND", "Follower is deleted");

        // Rename inbox folder if fullName changed and folder is not a system folder
        if (oldName != follower.FullName)
        {
            var inboxFolder = await _folderRepository.GetByIdForOwnerAsync(
                follower.InboxFolderId, request.OwnerId, cancellationToken);

            if (inboxFolder is not null && !inboxFolder.IsSystemFolder)
            {
                inboxFolder.Rename(follower.FullName);
            }
        }

        await _followerRepository.SaveChangesAsync(cancellationToken);

        return Result<ProjectFollowerDto>.Success(
            ListProjectFollowersQueryHandler.MapToDto(follower));
    }
}
