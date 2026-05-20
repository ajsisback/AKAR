namespace Akar.Application.DTOs;

public record ProjectDto(
    Guid Id,
    Guid OwnerId,
    string ProjectName,
    string ProjectType,
    string? City,
    string? LocationText,
    string? MapLink,
    string CurrentStage,
    string? OptionalImageUrl,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
