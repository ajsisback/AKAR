using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Folders;

/// <summary>
/// Lists all active (non-deleted) folders for a project owned by the requesting owner.
/// Backfills default system folders if they don't exist yet (handles existing projects).
/// Includes FileCount (active files only) for each folder.
/// </summary>
public record ListFoldersQuery(Guid ProjectId, Guid OwnerId) : IRequest<Result<List<ProjectFolderDto>>>;

public class ListFoldersQueryHandler : IRequestHandler<ListFoldersQuery, Result<List<ProjectFolderDto>>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFolderRepository _folderRepository;
    private readonly IProjectFileRepository _fileRepository;

    public ListFoldersQueryHandler(
        IProjectRepository projectRepository,
        IProjectFolderRepository folderRepository,
        IProjectFileRepository fileRepository)
    {
        _projectRepository = projectRepository;
        _folderRepository = folderRepository;
        _fileRepository = fileRepository;
    }

    public async Task<Result<List<ProjectFolderDto>>> Handle(ListFoldersQuery request, CancellationToken cancellationToken)
    {
        // Verify project belongs to owner
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
        {
            return Result<List<ProjectFolderDto>>.Failure("PROJECT_NOT_FOUND", "Project not found");
        }

        // Backfill default system folders for existing projects if missing
        var hasSystemFolders = await _folderRepository.HasSystemFoldersAsync(request.ProjectId, cancellationToken);
        if (!hasSystemFolders)
        {
            var defaultFolders = ProjectFolder.DefaultSystemFolders
                .Select(f => ProjectFolder.CreateSystemFolder(project.Id, request.OwnerId, f.Type, f.Name))
                .ToList();

            await _folderRepository.AddRangeAsync(defaultFolders, cancellationToken);
            await _folderRepository.SaveChangesAsync(cancellationToken);
        }

        var folders = await _folderRepository.GetByProjectIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);

        var dtos = new List<ProjectFolderDto>(folders.Count);
        foreach (var f in folders)
        {
            var fileCount = await _fileRepository.CountActiveByFolderAsync(f.Id, cancellationToken);
            dtos.Add(MapToDto(f, fileCount));
        }

        return Result<List<ProjectFolderDto>>.Success(dtos);
    }

    private static ProjectFolderDto MapToDto(ProjectFolder f, int fileCount) => new(
        f.Id, f.ProjectId, f.OwnerId, f.ParentFolderId,
        f.FolderName, f.FolderType.ToString(), f.IsSystemFolder,
        fileCount, f.CreatedAtUtc, f.UpdatedAtUtc);
}
