namespace Akar.Application.DTOs;

public record ProjectTimelineEventDto(
    Guid Id,
    Guid ProjectId,
    string EventType,
    string Stage,
    string Title,
    string? Description,
    DateTime EventDateUtc,
    string SourceType,
    Guid? SourceId,
    bool IsSystemGenerated,
    DateTime CreatedAtUtc);

public record ProjectStageDto(
    Guid ProjectId,
    string CurrentStage,
    DateTime? LastStageChangedAtUtc);
