using Akar.Application.Interfaces;
using Akar.Shared;
using Akar.Shared.Abstractions;
using MediatR;

namespace Akar.Application.Features.Files;

/// <summary>
/// Result type for file download — carries stream, content type, and original filename.
/// Never exposes storage path.
/// </summary>
public record FileDownloadResult(Stream Stream, string ContentType, string OriginalFileName);

public record DownloadProjectFileQuery(
    Guid ProjectId,
    Guid FileId,
    Guid OwnerId) : IRequest<Result<FileDownloadResult>>;

public class DownloadProjectFileQueryHandler : IRequestHandler<DownloadProjectFileQuery, Result<FileDownloadResult>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFileRepository _fileRepository;
    private readonly IFileStorageService _storageService;

    public DownloadProjectFileQueryHandler(
        IProjectRepository projectRepository,
        IProjectFileRepository fileRepository,
        IFileStorageService storageService)
    {
        _projectRepository = projectRepository;
        _fileRepository = fileRepository;
        _storageService = storageService;
    }

    public async Task<Result<FileDownloadResult>> Handle(DownloadProjectFileQuery request, CancellationToken cancellationToken)
    {
        // 1. Validate owner owns project
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<FileDownloadResult>.Failure("PROJECT_NOT_FOUND", "Project not found");

        // 2. Validate file exists, belongs to owner, belongs to project
        var file = await _fileRepository.GetByIdForOwnerAsync(request.FileId, request.OwnerId, cancellationToken);
        if (file is null || file.ProjectId != request.ProjectId)
            return Result<FileDownloadResult>.Failure("FILE_NOT_FOUND", "File not found");

        // 3. Do not allow downloading deleted files in Sprint 2B
        if (file.IsDeleted)
            return Result<FileDownloadResult>.Failure("FILE_NOT_FOUND", "File not found");

        // 4. Open stream from storage
        var stream = await _storageService.OpenReadAsync(file.StoragePath, cancellationToken);
        if (stream is null)
            return Result<FileDownloadResult>.Failure("FILE_NOT_FOUND", "File not found on storage");

        return Result<FileDownloadResult>.Success(new FileDownloadResult(stream, file.ContentType, file.OriginalFileName));
    }
}
