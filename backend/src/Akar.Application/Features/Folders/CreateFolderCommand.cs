using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Folders;

public record CreateFolderCommand(
    Guid ProjectId,
    Guid OwnerId,
    string FolderName,
    Guid? ParentFolderId) : IRequest<Result<ProjectFolderDto>>;

public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, Result<ProjectFolderDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFolderRepository _folderRepository;

    public CreateFolderCommandHandler(
        IProjectRepository projectRepository,
        IProjectFolderRepository folderRepository)
    {
        _projectRepository = projectRepository;
        _folderRepository = folderRepository;
    }

    public async Task<Result<ProjectFolderDto>> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
        {
            return Result<ProjectFolderDto>.Failure("PROJECT_NOT_FOUND", "Project not found");
        }

        if (string.IsNullOrWhiteSpace(request.FolderName))
        {
            return Result<ProjectFolderDto>.Failure("FOLDER_NAME_REQUIRED", "Folder name is required");
        }

        var folder = ProjectFolder.CreateCustomFolder(
            request.ProjectId,
            request.OwnerId,
            request.FolderName.Trim(),
            request.ParentFolderId);

        await _folderRepository.AddAsync(folder, cancellationToken);
        await _folderRepository.SaveChangesAsync(cancellationToken);

        return Result<ProjectFolderDto>.Success(MapToDto(folder));
    }

    private static ProjectFolderDto MapToDto(ProjectFolder f) => new(
        f.Id, f.ProjectId, f.OwnerId, f.ParentFolderId,
        f.FolderName, f.FolderType.ToString(), f.IsSystemFolder,
        f.CreatedAtUtc, f.UpdatedAtUtc);
}
