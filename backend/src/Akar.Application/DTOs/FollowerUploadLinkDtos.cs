namespace Akar.Application.DTOs;

/// <summary>Displayed in link lists (no raw token).</summary>
public record FollowerUploadLinkDto(
    Guid Id,
    Guid ProjectId,
    Guid FollowerId,
    string? TokenPreview,
    DateTime? ExpiresAtUtc,
    bool IsActive,
    bool IsRevoked,
    DateTime CreatedAtUtc,
    DateTime? LastUsedAtUtc);

/// <summary>Returned once at link generation — contains the raw token.</summary>
public record GenerateFollowerUploadLinkResponseDto(
    Guid UploadLinkId,
    string UploadToken,
    string UploadUrl,
    DateTime? ExpiresAtUtc);

/// <summary>Minimal safe info for the public upload page.</summary>
public record FollowerUploadInfoDto(
    string FollowerName,
    string ProjectName,
    string[] AllowedFileExtensions,
    long MaxFileSizeBytes,
    DateTime? ExpiresAtUtc);

/// <summary>Result of a public follower upload.</summary>
public record FollowerPublicUploadResultDto(
    Guid FileId,
    string OriginalFileName,
    string FileCategory,
    long FileSizeBytes,
    DateTime UploadedAtUtc);
