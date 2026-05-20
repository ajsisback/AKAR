namespace Akar.Application.DTOs;

public record DashboardDto(
    int TotalProjects,
    int NotStartedCount,
    int StructuralCount,
    int FinishingCount,
    int CompletedCount);
