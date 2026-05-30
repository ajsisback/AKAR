using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Files;

public record GetFolderFilesQuery(
    Guid ProjectId,
    Guid FolderId,
    Guid OwnerId) : IRequest<Result<List<ProjectFileDto>>>;

public class GetFolderFilesQueryHandler : IRequestHandler<GetFolderFilesQuery, Result<List<ProjectFileDto>>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFolderRepository _folderRepository;
    private readonly IProjectFileRepository _fileRepository;

    public GetFolderFilesQueryHandler(
        IProjectRepository projectRepository,
        IProjectFolderRepository folderRepository,
        IProjectFileRepository fileRepository)
    {
        _projectRepository = projectRepository;
        _folderRepository = folderRepository;
        _fileRepository = fileRepository;
    }

    public async Task<Result<List<ProjectFileDto>>> Handle(GetFolderFilesQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<List<ProjectFileDto>>.Failure("PROJECT_NOT_FOUND", "Project not found");

        var folder = await _folderRepository.GetByIdForOwnerAsync(request.FolderId, request.OwnerId, cancellationToken);
        if (folder is null || folder.ProjectId != request.ProjectId)
            return Result<List<ProjectFileDto>>.Failure("FOLDER_NOT_FOUND", "Folder not found");

        var files = await _fileRepository.GetActiveByFolderForOwnerAsync(request.FolderId, request.OwnerId, cancellationToken);

        var dtos = files.Select(MapToDto).ToList();
        return Result<List<ProjectFileDto>>.Success(dtos);
    }

    private static ProjectFileDto MapToDto(ProjectFile f) => new(
        f.Id, f.ProjectId, f.FolderId, f.OriginalFileName,
        f.ContentType, f.FileExtension, f.FileSizeBytes,
        f.FileCategory.ToString(), f.IsDeleted, f.DeletedAtUtc,
        f.CreatedAtUtc, f.UpdatedAtUtc);
}
