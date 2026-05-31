using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Application.Services;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Shared;
using Akar.Shared.Abstractions;
using MediatR;

namespace Akar.Application.Features.UploadLinks;

public record UploadFollowerFileCommand(
    string RawToken,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    Stream FileStream) : IRequest<Result<FollowerPublicUploadResultDto>>;

public class UploadFollowerFileCommandHandler
    : IRequestHandler<UploadFollowerFileCommand, Result<FollowerPublicUploadResultDto>>
{
    private readonly IFollowerUploadLinkRepository _linkRepository;
    private readonly IProjectFollowerRepository _followerRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFolderRepository _folderRepository;
    private readonly IProjectFileRepository _fileRepository;
    private readonly IFileStorageService _storageService;
    private readonly IProjectTimelineEventWriter _timelineWriter;

    public UploadFollowerFileCommandHandler(
        IFollowerUploadLinkRepository linkRepository,
        IProjectFollowerRepository followerRepository,
        IProjectRepository projectRepository,
        IProjectFolderRepository folderRepository,
        IProjectFileRepository fileRepository,
        IFileStorageService storageService,
        IProjectTimelineEventWriter timelineWriter)
    {
        _linkRepository = linkRepository;
        _followerRepository = followerRepository;
        _projectRepository = projectRepository;
        _folderRepository = folderRepository;
        _fileRepository = fileRepository;
        _storageService = storageService;
        _timelineWriter = timelineWriter;
    }

    public async Task<Result<FollowerPublicUploadResultDto>> Handle(
        UploadFollowerFileCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate token
        var tokenHash = UploadTokenService.HashToken(request.RawToken);
        var link = await _linkRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (link is null)
            return Result<FollowerPublicUploadResultDto>.Failure("UPLOAD_LINK_NOT_FOUND", "Upload link not found");
        if (link.IsRevoked)
            return Result<FollowerPublicUploadResultDto>.Failure("UPLOAD_LINK_REVOKED", "Upload link has been revoked");
        if (link.ExpiresAtUtc.HasValue && link.ExpiresAtUtc.Value < DateTime.UtcNow)
            return Result<FollowerPublicUploadResultDto>.Failure("UPLOAD_LINK_EXPIRED", "Upload link has expired");
        if (!link.IsActive)
            return Result<FollowerPublicUploadResultDto>.Failure("UPLOAD_LINK_NOT_FOUND", "Upload link is inactive");

        // 2. Validate follower
        var follower = await _followerRepository.GetByIdForOwnerAsync(
            link.FollowerId, link.OwnerId, cancellationToken);
        if (follower is null)
            return Result<FollowerPublicUploadResultDto>.Failure("FOLLOWER_NOT_FOUND", "Follower not found");
        if (!follower.IsActive || follower.IsDeleted)
            return Result<FollowerPublicUploadResultDto>.Failure("FOLLOWER_INACTIVE", "Follower is inactive");

        // 3. Validate project
        var project = await _projectRepository.GetByIdAsync(link.ProjectId, cancellationToken);
        if (project is null)
            return Result<FollowerPublicUploadResultDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        // 4. Validate file
        if (request.FileStream is null || request.FileSizeBytes == 0)
            return Result<FollowerPublicUploadResultDto>.Failure("FILE_REQUIRED", "A file is required");

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
            return Result<FollowerPublicUploadResultDto>.Failure("FILE_REQUIRED", "File name is required");

        var originalFileName = FileValidationService.SanitizeFileName(request.OriginalFileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (FileValidationService.BlockedExtensions.Contains(extension))
            return Result<FollowerPublicUploadResultDto>.Failure("FILE_TYPE_NOT_ALLOWED", $"File type '{extension}' is not allowed");

        if (!FileValidationService.AllowedExtensions.TryGetValue(extension, out var extInfo))
            return Result<FollowerPublicUploadResultDto>.Failure("FILE_TYPE_NOT_ALLOWED", $"File type '{extension}' is not allowed");

        if (request.FileSizeBytes > extInfo.MaxBytes)
        {
            var maxMb = extInfo.MaxBytes / (1024 * 1024);
            return Result<FollowerPublicUploadResultDto>.Failure("FILE_TOO_LARGE", $"File exceeds maximum size of {maxMb} MB");
        }

        // 5. Save physical file to follower's inbox folder
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        string storagePath;

        try
        {
            storagePath = await _storageService.SaveAsync(
                link.OwnerId, link.ProjectId, follower.InboxFolderId,
                storedFileName, request.FileStream, cancellationToken);
        }
        catch (Exception)
        {
            return Result<FollowerPublicUploadResultDto>.Failure("STORAGE_SAVE_FAILED", "Failed to save file to storage");
        }

        // 6. Create file metadata
        var contentType = request.ContentType ?? "application/octet-stream";
        var projectFile = ProjectFile.Create(
            link.ProjectId, link.OwnerId, follower.InboxFolderId,
            originalFileName, storedFileName, contentType, extension,
            request.FileSizeBytes, StorageProvider.Local, storagePath,
            extInfo.Category);

        await _fileRepository.AddAsync(projectFile, cancellationToken);

        // 7. Update link usage
        link.RecordUsage();

        await _fileRepository.SaveChangesAsync(cancellationToken);
        await _linkRepository.SaveChangesAsync(cancellationToken);

        // 8. Create timeline event (scoped to project owner)
        await _timelineWriter.AddSystemEventAsync(
            link.ProjectId, link.OwnerId, project.CurrentStage,
            TimelineEventType.FollowerFileUploaded, TimelineSourceType.ProjectFile, projectFile.Id,
            "Follower file uploaded", $"{follower.FullName}: {originalFileName}",
            cancellationToken);

        return Result<FollowerPublicUploadResultDto>.Success(
            new FollowerPublicUploadResultDto(
                projectFile.Id, originalFileName, extInfo.Category.ToString(),
                request.FileSizeBytes, projectFile.CreatedAtUtc));
    }
}
