using Akar.Application.Features.Timeline;
using Akar.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}")]
[Authorize]
public class ProjectTimelineController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectTimelineController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns active timeline events for the project.</summary>
    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline(
        Guid projectId,
        [FromQuery] string? stage,
        [FromQuery] string? eventType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        CurrentStage? stageFilter = null;
        if (!string.IsNullOrEmpty(stage) && Enum.TryParse<CurrentStage>(stage, true, out var s))
            stageFilter = s;

        TimelineEventType? typeFilter = null;
        if (!string.IsNullOrEmpty(eventType) && Enum.TryParse<TimelineEventType>(eventType, true, out var t))
            typeFilter = t;

        var result = await _mediator.Send(
            new GetProjectTimelineQuery(projectId, ownerId.Value, stageFilter, typeFilter, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Creates a manual timeline note.</summary>
    [HttpPost("timeline/notes")]
    public async Task<IActionResult> AddNote(
        Guid projectId,
        [FromBody] AddTimelineNoteRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new AddProjectTimelineNoteCommand(
            projectId, ownerId.Value,
            request.Stage,
            request.Title,
            request.Description,
            request.EventDateUtc);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetTimeline), new { projectId }, result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Soft-deletes a manual timeline event.</summary>
    [HttpDelete("timeline/{eventId:guid}")]
    public async Task<IActionResult> Delete(
        Guid projectId,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new DeleteProjectTimelineEventCommand(projectId, eventId, ownerId.Value),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Updates the project current stage.</summary>
    [HttpPut("stage")]
    public async Task<IActionResult> UpdateStage(
        Guid projectId,
        [FromBody] UpdateStageRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new UpdateProjectStageCommand(
            projectId, ownerId.Value,
            request.CurrentStage,
            request.Note);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Returns the current project stage info.</summary>
    [HttpGet("stage")]
    public async Task<IActionResult> GetStage(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new GetProjectStageQuery(projectId, ownerId.Value),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    // ── Helpers ──────────────────────────────────────────

    private Guid? GetOwnerId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private IActionResult MapError(string code, string detail) => code switch
    {
        "PROJECT_NOT_FOUND" or "TIMELINE_EVENT_NOT_FOUND"
            => NotFound(new ProblemDetails { Status = 404, Title = code, Detail = detail }),
        "TIMELINE_SYSTEM_EVENT_CANNOT_BE_DELETED"
            => Conflict(new ProblemDetails { Status = 409, Title = code, Detail = detail }),
        "STAGE_ALREADY_SET"
            => Conflict(new ProblemDetails { Status = 409, Title = code, Detail = detail }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = code, Detail = detail })
    };
}

public record AddTimelineNoteRequest(
    string Stage,
    string Title,
    string? Description,
    DateTime? EventDateUtc);

public record UpdateStageRequest(
    string CurrentStage,
    string? Note);
