using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Files;

/// <summary>
/// Lists deleted files and deleted custom folders for a project.
/// System folders are never included because they should never be deleted.
/// Behavior note: If a custom folder is soft-deleted, its files are NOT cascaded —
/// files remain active but are hidden from normal folder listing since the folder
/// itself won't appear. They are discoverable through the trash endpoint if
/// individually deleted.
/// </summary>
public record GetProjectTrashQuery(
    Guid ProjectId,
    Guid OwnerId) : IRequest<Result<ProjectTrashDto>>;

public class GetProjectTrashQueryHandler : IRequestHandler<GetProjectTrashQuery, Result<ProjectTrashDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFileRepository _fileRepository;
    private readonly IProjectFolderRepository _folderRepository;

    public GetProjectTrashQueryHandler(
        IProjectRepository projectRepository,
        IProjectFileRepository fileRepository,
        IProjectFolderRepository folderRepository)
    {
        _projectRepository = projectRepository;
        _fileRepository = fileRepository;
        _folderRepository = folderRepository;
    }

    public async Task<Result<ProjectTrashDto>> Handle(GetProjectTrashQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<ProjectTrashDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        // Get deleted files
        var deletedFiles = await _fileRepository.GetDeletedByProjectForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        var fileDtos = deletedFiles.Select(MapFileToDto).ToList();

        // Get deleted custom folders (not system folders — system folders should never be deleted)
        var allFolders = await _folderRepository.GetDeletedByProjectForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        var folderDtos = allFolders
            .Where(f => !f.IsSystemFolder)
            .Select(f => MapFolderToDto(f))
            .ToList();

        return Result<ProjectTrashDto>.Success(new ProjectTrashDto(fileDtos, folderDtos));
    }

    private static ProjectFileDto MapFileToDto(ProjectFile f) => new(
        f.Id, f.ProjectId, f.FolderId, f.OriginalFileName,
        f.ContentType, f.FileExtension, f.FileSizeBytes,
        f.FileCategory.ToString(), f.IsDeleted, f.DeletedAtUtc,
        f.CreatedAtUtc, f.UpdatedAtUtc);

    private static ProjectFolderDto MapFolderToDto(ProjectFolder f) => new(
        f.Id, f.ProjectId, f.OwnerId, f.ParentFolderId,
        f.FolderName, f.FolderType.ToString(), f.IsSystemFolder,
        0, f.CreatedAtUtc, f.UpdatedAtUtc);
}
