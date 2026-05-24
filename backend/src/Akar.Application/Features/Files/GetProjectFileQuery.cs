using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Files;

public record GetProjectFileQuery(
    Guid ProjectId,
    Guid FileId,
    Guid OwnerId) : IRequest<Result<ProjectFileDto>>;

public class GetProjectFileQueryHandler : IRequestHandler<GetProjectFileQuery, Result<ProjectFileDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFileRepository _fileRepository;

    public GetProjectFileQueryHandler(
        IProjectRepository projectRepository,
        IProjectFileRepository fileRepository)
    {
        _projectRepository = projectRepository;
        _fileRepository = fileRepository;
    }

    public async Task<Result<ProjectFileDto>> Handle(GetProjectFileQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<ProjectFileDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        var file = await _fileRepository.GetByIdForOwnerAsync(request.FileId, request.OwnerId, cancellationToken);
        if (file is null || file.ProjectId != request.ProjectId)
            return Result<ProjectFileDto>.Failure("FILE_NOT_FOUND", "File not found");

        return Result<ProjectFileDto>.Success(MapToDto(file));
    }

    private static ProjectFileDto MapToDto(ProjectFile f) => new(
        f.Id, f.ProjectId, f.FolderId, f.OriginalFileName,
        f.ContentType, f.FileExtension, f.FileSizeBytes,
        f.FileCategory.ToString(), f.IsDeleted, f.DeletedAtUtc,
        f.CreatedAtUtc, f.UpdatedAtUtc);
}
