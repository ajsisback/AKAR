namespace Akar.Application.DTOs;

public record ProjectSettingsDto(
    Guid ProjectId,
    string ProjectName,
    string ProjectType,
    string CurrentStage,
    string? City,
    string? LocationText,
    string? MapLink,
    DateTime UpdatedAtUtc);
