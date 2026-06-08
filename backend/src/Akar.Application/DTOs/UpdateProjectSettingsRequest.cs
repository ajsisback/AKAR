using Akar.Domain.Enums;

namespace Akar.Application.DTOs;

public record UpdateProjectSettingsRequest(
    string ProjectName,
    string ProjectType,
    string? City,
    string? LocationText,
    string? MapLink);
