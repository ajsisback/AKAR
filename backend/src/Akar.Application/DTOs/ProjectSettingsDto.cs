using Akar.Domain.Enums;

namespace Akar.Application.DTOs;

public record ProjectSettingsDto(
    Guid ProjectId,
    string ProjectName,
    ProjectType ProjectType,
    CurrentStage CurrentStage,
    string? City,
    string? LocationText,
    string? MapUrl,
    DateTime UpdatedAtUtc);
