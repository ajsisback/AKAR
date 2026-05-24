using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Folders;

public record RenameFolderCommand(
    Guid ProjectId,
    Guid FolderId,
    Guid OwnerId,
    string NewName) : IRequest<Result<ProjectFolderDto>>;

public class RenameFolderCommandHandler : IRequestHandler<RenameFolderCommand, Result<ProjectFolderDto>>
{
    private readonly IProjectFolderRepository _folderRepository;

    public RenameFolderCommandHandler(IProjectFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<Result<ProjectFolderDto>> Handle(RenameFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await _folderRepository.GetByIdForOwnerAsync(request.FolderId, request.OwnerId, cancellationToken);
        if (folder is null || folder.ProjectId != request.ProjectId)
        {
            return Result<ProjectFolderDto>.Failure("FOLDER_NOT_FOUND", "Folder not found");
        }

        if (string.IsNullOrWhiteSpace(request.NewName))
        {
            return Result<ProjectFolderDto>.Failure("FOLDER_NAME_REQUIRED", "Folder name is required");
        }

        if (!folder.Rename(request.NewName.Trim()))
        {
            return Result<ProjectFolderDto>.Failure("FOLDER_SYSTEM_PROTECTED", "System folders cannot be renamed");
        }

        await _folderRepository.SaveChangesAsync(cancellationToken);

        return Result<ProjectFolderDto>.Success(MapToDto(folder));
    }

    private static ProjectFolderDto MapToDto(ProjectFolder f) => new(
        f.Id, f.ProjectId, f.OwnerId, f.ParentFolderId,
        f.FolderName, f.FolderType.ToString(), f.IsSystemFolder,
        0, f.CreatedAtUtc, f.UpdatedAtUtc);
}
