namespace Akar.Application.DTOs;

public record ProjectFollowerDto(
    Guid Id,
    Guid ProjectId,
    Guid OwnerId,
    Guid InboxFolderId,
    string FullName,
    string Phone,
    string FollowerType,
    string? Notes,
    bool IsActive,
    bool IsDeleted,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
