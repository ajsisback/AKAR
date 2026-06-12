namespace Akar.Application.DTOs;

// --- Admin Auth ---

public record AdminAuthResponseDto(
    string Token,
    AdminDto Admin);

public record AdminLoginRequest(
    string Email,
    string Password);

public record AdminDto(
    Guid Id,
    string FullName,
    string Email,
    string Role);

// --- Admin read-only views ---

public record AdminOwnerListItemDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int ProjectsCount);

public record AdminOwnerDetailDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    List<AdminOwnerProjectSummaryDto> Projects);

public record AdminOwnerProjectSummaryDto(
    Guid ProjectId,
    string ProjectName,
    string ProjectType,
    string CurrentStage,
    string? City,
    DateTime CreatedAtUtc);

public record AdminProjectListItemDto(
    Guid ProjectId,
    Guid OwnerId,
    string OwnerName,
    string ProjectName,
    string ProjectType,
    string CurrentStage,
    string? City,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record AdminProjectDetailDto(
    Guid ProjectId,
    Guid OwnerId,
    string OwnerName,
    string ProjectName,
    string ProjectType,
    string CurrentStage,
    string? City,
    string? LocationText,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int FileCount,
    int FollowerCount,
    int ContractCount,
    int TimelineCount);
