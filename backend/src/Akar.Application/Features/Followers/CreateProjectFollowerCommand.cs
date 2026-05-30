using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Followers;

public record CreateProjectFollowerCommand(
    Guid ProjectId,
    Guid OwnerId,
    string FullName,
    string Phone,
    string FollowerType,
    string? Notes) : IRequest<Result<ProjectFollowerDto>>;

public class CreateProjectFollowerCommandHandler
    : IRequestHandler<CreateProjectFollowerCommand, Result<ProjectFollowerDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFolderRepository _folderRepository;
    private readonly IProjectFollowerRepository _followerRepository;

    public CreateProjectFollowerCommandHandler(
        IProjectRepository projectRepository,
        IProjectFolderRepository folderRepository,
        IProjectFollowerRepository followerRepository)
    {
        _projectRepository = projectRepository;
        _folderRepository = folderRepository;
        _followerRepository = followerRepository;
    }

    public async Task<Result<ProjectFollowerDto>> Handle(
        CreateProjectFollowerCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate project ownership
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);

        if (project is null)
            return Result<ProjectFollowerDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        // 2. Parse follower type
        if (!Enum.TryParse<FollowerType>(request.FollowerType, ignoreCase: true, out var followerType))
            return Result<ProjectFollowerDto>.Failure("INVALID_FOLLOWER_TYPE", "Invalid follower type");

        // 3. Check phone uniqueness within project
        if (await _followerRepository.ExistsByPhoneInProjectAsync(
                request.ProjectId, request.Phone.Trim(), cancellationToken))
            return Result<ProjectFollowerDto>.Failure("FOLLOWER_PHONE_ALREADY_EXISTS",
                "A follower with this phone number already exists in the project");

        // 4. Find FollowersInbox system folder
        var folders = await _folderRepository.GetByProjectIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);

        var followersInbox = folders.FirstOrDefault(
            f => f.FolderType == FolderType.FollowersInbox && f.IsSystemFolder);

        if (followersInbox is null)
        {
            // Ensure default system folders exist
            if (!await _folderRepository.HasSystemFoldersAsync(request.ProjectId, cancellationToken))
            {
                var systemFolders = ProjectFolder.DefaultSystemFolders
                    .Select(sf => ProjectFolder.CreateSystemFolder(
                        request.ProjectId, request.OwnerId, sf.Type, sf.Name));
                await _folderRepository.AddRangeAsync(systemFolders, cancellationToken);
                await _folderRepository.SaveChangesAsync(cancellationToken);

                // Re-fetch
                folders = await _folderRepository.GetByProjectIdForOwnerAsync(
                    request.ProjectId, request.OwnerId, cancellationToken);
                followersInbox = folders.FirstOrDefault(
                    f => f.FolderType == FolderType.FollowersInbox && f.IsSystemFolder);
            }

            if (followersInbox is null)
                return Result<ProjectFollowerDto>.Failure("FOLLOWERS_INBOX_NOT_FOUND",
                    "FollowersInbox system folder not found");
        }

        // 5. Create follower inbox folder under FollowersInbox
        var inboxFolder = ProjectFolder.CreateCustomFolder(
            request.ProjectId,
            request.OwnerId,
            request.FullName.Trim(),
            followersInbox.Id);

        await _folderRepository.AddAsync(inboxFolder, cancellationToken);
        await _folderRepository.SaveChangesAsync(cancellationToken);

        // 6. Create the follower
        var follower = ProjectFollower.Create(
            request.ProjectId,
            request.OwnerId,
            inboxFolder.Id,
            request.FullName,
            request.Phone,
            followerType,
            request.Notes);

        await _followerRepository.AddAsync(follower, cancellationToken);
        await _followerRepository.SaveChangesAsync(cancellationToken);

        return Result<ProjectFollowerDto>.Success(
            ListProjectFollowersQueryHandler.MapToDto(follower));
    }
}
