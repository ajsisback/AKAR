namespace Akar.Application.DTOs;

public record ProjectFileDto(
    Guid Id,
    Guid ProjectId,
    Guid FolderId,
    string OriginalFileName,
    string ContentType,
    string FileExtension,
    long FileSizeBytes,
    string FileCategory,
    bool IsDeleted,
    DateTime? DeletedAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
