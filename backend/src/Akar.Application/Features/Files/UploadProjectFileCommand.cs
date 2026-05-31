using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Shared;
using Akar.Shared.Abstractions;
using MediatR;

namespace Akar.Application.Features.Files;

/// <summary>
/// Clean command model — no ASP.NET Core dependencies.
/// The controller extracts Stream/OriginalFileName/ContentType/FileSizeBytes from IFormFile.
/// </summary>
public record UploadProjectFileCommand(
    Guid ProjectId,
    Guid FolderId,
    Guid OwnerId,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    Stream FileStream) : IRequest<Result<ProjectFileDto>>;

public class UploadProjectFileCommandHandler : IRequestHandler<UploadProjectFileCommand, Result<ProjectFileDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFolderRepository _folderRepository;
    private readonly IProjectFileRepository _fileRepository;
    private readonly IFileStorageService _storageService;
    private readonly IProjectTimelineEventWriter _timelineWriter;

    // Allowed extensions by category
    private static readonly Dictionary<string, (FileCategory Category, long MaxBytes)> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Documents — 20 MB
        { ".pdf",  (FileCategory.Document, 20 * 1024 * 1024) },
        { ".doc",  (FileCategory.Document, 20 * 1024 * 1024) },
        { ".docx", (FileCategory.Document, 20 * 1024 * 1024) },
        { ".xls",  (FileCategory.Document, 20 * 1024 * 1024) },
        { ".xlsx", (FileCategory.Document, 20 * 1024 * 1024) },
        // Images — 10 MB
        { ".jpg",  (FileCategory.Image, 10 * 1024 * 1024) },
        { ".jpeg", (FileCategory.Image, 10 * 1024 * 1024) },
        { ".png",  (FileCategory.Image, 10 * 1024 * 1024) },
        { ".webp", (FileCategory.Image, 10 * 1024 * 1024) },
        // Videos — 100 MB
        { ".mp4",  (FileCategory.Video, 100 * 1024 * 1024) },
        { ".mov",  (FileCategory.Video, 100 * 1024 * 1024) },
    };

    // Explicitly blocked extensions
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".ps1", ".sh", ".js", ".msi", ".dll", ".zip"
    };

    public UploadProjectFileCommandHandler(
        IProjectRepository projectRepository,
        IProjectFolderRepository folderRepository,
        IProjectFileRepository fileRepository,
        IFileStorageService storageService,
        IProjectTimelineEventWriter timelineWriter)
    {
        _projectRepository = projectRepository;
        _folderRepository = folderRepository;
        _fileRepository = fileRepository;
        _storageService = storageService;
        _timelineWriter = timelineWriter;
    }

    public async Task<Result<ProjectFileDto>> Handle(UploadProjectFileCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate project ownership
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<ProjectFileDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        // 2. Validate folder ownership and belongs to project
        var folder = await _folderRepository.GetByIdForOwnerAsync(request.FolderId, request.OwnerId, cancellationToken);
        if (folder is null || folder.ProjectId != request.ProjectId)
            return Result<ProjectFileDto>.Failure("FOLDER_NOT_FOUND", "Folder not found");

        // 3. Validate file is present
        if (request.FileStream is null || request.FileSizeBytes == 0)
            return Result<ProjectFileDto>.Failure("FILE_REQUIRED", "A file is required");

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
            return Result<ProjectFileDto>.Failure("FILE_REQUIRED", "File name is required");

        var originalFileName = SanitizeFileName(request.OriginalFileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        // 4. Check blocked extensions first
        if (BlockedExtensions.Contains(extension))
            return Result<ProjectFileDto>.Failure("FILE_TYPE_NOT_ALLOWED", $"File type '{extension}' is not allowed");

        // 5. Check allowed extensions
        if (!AllowedExtensions.TryGetValue(extension, out var extInfo))
            return Result<ProjectFileDto>.Failure("FILE_TYPE_NOT_ALLOWED", $"File type '{extension}' is not allowed");

        // 6. Check file size
        if (request.FileSizeBytes > extInfo.MaxBytes)
        {
            var maxMb = extInfo.MaxBytes / (1024 * 1024);
            return Result<ProjectFileDto>.Failure("FILE_TOO_LARGE", $"File exceeds maximum size of {maxMb} MB for {extInfo.Category} files");
        }

        // 7. Save physical file
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        string storagePath;

        try
        {
            storagePath = await _storageService.SaveAsync(
                request.OwnerId, request.ProjectId, request.FolderId,
                storedFileName, request.FileStream, cancellationToken);
        }
        catch (Exception)
        {
            return Result<ProjectFileDto>.Failure("STORAGE_SAVE_FAILED", "Failed to save file to storage");
        }

        // 8. Create metadata entity
        var contentType = request.ContentType ?? "application/octet-stream";

        var projectFile = ProjectFile.Create(
            request.ProjectId,
            request.OwnerId,
            request.FolderId,
            originalFileName,
            storedFileName,
            contentType,
            extension,
            request.FileSizeBytes,
            StorageProvider.Local,
            storagePath,
            extInfo.Category);

        await _fileRepository.AddAsync(projectFile, cancellationToken);
        await _fileRepository.SaveChangesAsync(cancellationToken);

        // 9. Create timeline event
        await _timelineWriter.AddSystemEventAsync(
            project.Id, project.OwnerId, project.CurrentStage,
            TimelineEventType.FileUploaded, TimelineSourceType.ProjectFile, projectFile.Id,
            "File uploaded", $"File uploaded: {originalFileName}",
            cancellationToken);

        return Result<ProjectFileDto>.Success(MapToDto(projectFile));
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove path separators that could be used for traversal
        var name = Path.GetFileName(fileName);
        // Remove any remaining dangerous characters
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            name = name.Replace(c, '_');
        }
        return string.IsNullOrWhiteSpace(name) ? "unnamed" : name;
    }

    private static ProjectFileDto MapToDto(ProjectFile f) => new(
        f.Id, f.ProjectId, f.FolderId, f.OriginalFileName,
        f.ContentType, f.FileExtension, f.FileSizeBytes,
        f.FileCategory.ToString(), f.IsDeleted, f.DeletedAtUtc,
        f.CreatedAtUtc, f.UpdatedAtUtc);
}
