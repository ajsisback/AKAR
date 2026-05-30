using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Timeline;

// ═══════════════════════════════════════════════════════════════
// Get project timeline events
// ═══════════════════════════════════════════════════════════════

public record GetProjectTimelineQuery(
    Guid ProjectId, Guid OwnerId,
    CurrentStage? Stage = null,
    TimelineEventType? EventType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<Result<List<ProjectTimelineEventDto>>>;

public class GetProjectTimelineQueryHandler
    : IRequestHandler<GetProjectTimelineQuery, Result<List<ProjectTimelineEventDto>>>
{
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectTimelineRepository _repo;

    public GetProjectTimelineQueryHandler(
        IProjectRepository projectRepo,
        IProjectTimelineRepository repo)
    {
        _projectRepo = projectRepo;
        _repo = repo;
    }

    public async Task<Result<List<ProjectTimelineEventDto>>> Handle(
        GetProjectTimelineQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepo.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<List<ProjectTimelineEventDto>>.Failure("PROJECT_NOT_FOUND", "Project not found");

        var events = await _repo.GetActiveByProjectForOwnerAsync(
            request.ProjectId, request.OwnerId,
            request.Stage, request.EventType,
            request.FromDate, request.ToDate,
            cancellationToken);

        var dtos = events.Select(MapToDto).ToList();
        return Result<List<ProjectTimelineEventDto>>.Success(dtos);
    }

    public static ProjectTimelineEventDto MapToDto(ProjectTimelineEvent e) => new(
        e.Id, e.ProjectId,
        e.EventType.ToString(), e.Stage.ToString(),
        e.Title, e.Description,
        e.EventDateUtc,
        e.SourceType.ToString(), e.SourceId,
        e.IsSystemGenerated,
        e.CreatedAtUtc);
}

// ═══════════════════════════════════════════════════════════════
// Get project stage info
// ═══════════════════════════════════════════════════════════════

public record GetProjectStageQuery(
    Guid ProjectId, Guid OwnerId) : IRequest<Result<ProjectStageDto>>;

public class GetProjectStageQueryHandler
    : IRequestHandler<GetProjectStageQuery, Result<ProjectStageDto>>
{
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectTimelineRepository _timelineRepo;

    public GetProjectStageQueryHandler(
        IProjectRepository projectRepo,
        IProjectTimelineRepository timelineRepo)
    {
        _projectRepo = projectRepo;
        _timelineRepo = timelineRepo;
    }

    public async Task<Result<ProjectStageDto>> Handle(
        GetProjectStageQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepo.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<ProjectStageDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        var lastChange = await _timelineRepo.GetLastStageChangeAsync(
            request.ProjectId, request.OwnerId, cancellationToken);

        return Result<ProjectStageDto>.Success(new ProjectStageDto(
            project.Id,
            project.CurrentStage.ToString(),
            lastChange));
    }
}

// ═══════════════════════════════════════════════════════════════
// Add manual timeline note
// ═══════════════════════════════════════════════════════════════

public record AddProjectTimelineNoteCommand(
    Guid ProjectId,
    Guid OwnerId,
    string Stage,
    string Title,
    string? Description,
    DateTime? EventDateUtc) : IRequest<Result<ProjectTimelineEventDto>>;

public class AddProjectTimelineNoteCommandHandler
    : IRequestHandler<AddProjectTimelineNoteCommand, Result<ProjectTimelineEventDto>>
{
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectTimelineRepository _repo;

    public AddProjectTimelineNoteCommandHandler(
        IProjectRepository projectRepo,
        IProjectTimelineRepository repo)
    {
        _projectRepo = projectRepo;
        _repo = repo;
    }

    public async Task<Result<ProjectTimelineEventDto>> Handle(
        AddProjectTimelineNoteCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate project ownership
        var project = await _projectRepo.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<ProjectTimelineEventDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        // 2. Validate title
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<ProjectTimelineEventDto>.Failure("TIMELINE_TITLE_REQUIRED", "Title is required");
        if (request.Title.Length > 200)
            return Result<ProjectTimelineEventDto>.Failure("TIMELINE_TITLE_REQUIRED", "Title must be 200 characters or less");

        // 3. Validate description
        if (request.Description is not null && request.Description.Length > 2000)
            return Result<ProjectTimelineEventDto>.Failure("VALIDATION_ERROR", "Description must be 2000 characters or less");

        // 4. Validate stage
        if (!Enum.TryParse<CurrentStage>(request.Stage, true, out var stage))
            return Result<ProjectTimelineEventDto>.Failure("INVALID_PROJECT_STAGE", "Invalid project stage");

        // 5. Validate future date (max 5 years)
        if (request.EventDateUtc.HasValue && request.EventDateUtc.Value > DateTime.UtcNow.AddYears(5))
            return Result<ProjectTimelineEventDto>.Failure("VALIDATION_ERROR", "Event date cannot be more than 5 years in the future");

        // 6. Create
        var timelineEvent = ProjectTimelineEvent.CreateManualNote(
            request.ProjectId,
            request.OwnerId,
            stage,
            request.Title.Trim(),
            request.Description?.Trim(),
            request.EventDateUtc);

        await _repo.AddAsync(timelineEvent, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return Result<ProjectTimelineEventDto>.Success(
            GetProjectTimelineQueryHandler.MapToDto(timelineEvent));
    }
}

// ═══════════════════════════════════════════════════════════════
// Delete (soft-delete) manual timeline event
// ═══════════════════════════════════════════════════════════════

public record DeleteProjectTimelineEventCommand(
    Guid ProjectId, Guid EventId, Guid OwnerId) : IRequest<Result<bool>>;

public class DeleteProjectTimelineEventCommandHandler
    : IRequestHandler<DeleteProjectTimelineEventCommand, Result<bool>>
{
    private readonly IProjectTimelineRepository _repo;

    public DeleteProjectTimelineEventCommandHandler(IProjectTimelineRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(
        DeleteProjectTimelineEventCommand request, CancellationToken cancellationToken)
    {
        var timelineEvent = await _repo.GetByIdForOwnerAsync(request.EventId, request.OwnerId, cancellationToken);
        if (timelineEvent is null || timelineEvent.ProjectId != request.ProjectId)
            return Result<bool>.Failure("TIMELINE_EVENT_NOT_FOUND", "Timeline event not found");

        if (timelineEvent.IsSystemGenerated)
            return Result<bool>.Failure("TIMELINE_SYSTEM_EVENT_CANNOT_BE_DELETED", "System-generated events cannot be deleted");

        timelineEvent.SoftDelete();
        await _repo.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

// ═══════════════════════════════════════════════════════════════
// Update project stage
// ═══════════════════════════════════════════════════════════════

public record UpdateProjectStageCommand(
    Guid ProjectId,
    Guid OwnerId,
    string CurrentStage,
    string? Note) : IRequest<Result<ProjectStageDto>>;

public class UpdateProjectStageCommandHandler
    : IRequestHandler<UpdateProjectStageCommand, Result<ProjectStageDto>>
{
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectTimelineRepository _timelineRepo;

    public UpdateProjectStageCommandHandler(
        IProjectRepository projectRepo,
        IProjectTimelineRepository timelineRepo)
    {
        _projectRepo = projectRepo;
        _timelineRepo = timelineRepo;
    }

    public async Task<Result<ProjectStageDto>> Handle(
        UpdateProjectStageCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate project ownership
        var project = await _projectRepo.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<ProjectStageDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        // 2. Validate stage
        if (!Enum.TryParse<CurrentStage>(request.CurrentStage, true, out var newStage))
            return Result<ProjectStageDto>.Failure("INVALID_PROJECT_STAGE", "Invalid project stage");

        // 3. Validate note length
        if (request.Note is not null && request.Note.Length > 1000)
            return Result<ProjectStageDto>.Failure("VALIDATION_ERROR", "Note must be 1000 characters or less");

        // 4. Check if same stage — return success without duplicate event
        if (project.CurrentStage == newStage)
            return Result<ProjectStageDto>.Failure("STAGE_ALREADY_SET", "Project is already at this stage");

        // 5. Update project stage
        project.UpdateStage(newStage);

        // 6. Create system timeline event
        var stageEvent = ProjectTimelineEvent.CreateStageChanged(
            request.ProjectId,
            request.OwnerId,
            newStage,
            request.Note?.Trim());

        await _timelineRepo.AddAsync(stageEvent, cancellationToken);
        await _timelineRepo.SaveChangesAsync(cancellationToken);

        return Result<ProjectStageDto>.Success(new ProjectStageDto(
            project.Id,
            project.CurrentStage.ToString(),
            stageEvent.EventDateUtc));
    }
}
