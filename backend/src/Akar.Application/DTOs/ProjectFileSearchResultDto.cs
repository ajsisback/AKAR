namespace Akar.Application.DTOs;

/// <summary>
/// Search result item for project file search.
/// Excludes sensitive storage internals (StoragePath, StoredFileName).
/// </summary>
public record ProjectFileSearchResultDto(
    Guid Id,
    Guid ProjectId,
    Guid FolderId,
    string FolderName,
    string OriginalFileName,
    string ContentType,
    string FileExtension,
    long FileSizeBytes,
    string FileCategory,
    bool IsDeleted,
    DateTime? DeletedAtUtc,
    DateTime CreatedAtUtc);
