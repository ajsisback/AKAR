namespace Akar.Application.DTOs;

public record ProjectFolderDto(
    Guid Id,
    Guid ProjectId,
    Guid OwnerId,
    Guid? ParentFolderId,
    string FolderName,
    string FolderType,
    bool IsSystemFolder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
