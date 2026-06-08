using Akar.Application.Features.Projects;
using Akar.Application.Features.ProjectSettings;
using Akar.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new CreateProjectCommand(
            ownerId.Value,
            request.ProjectName,
            request.ProjectType,
            request.City,
            request.LocationText,
            request.MapLink,
            request.CurrentStage,
            request.OptionalImageUrl);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
                Instance = HttpContext.Request.Path
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(new ListProjectsQuery(ownerId.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(new GetProjectByIdQuery(id, ownerId.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "PROJECT_NOT_FOUND"
                ? NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage })
                : BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/settings")]
    public async Task<ActionResult<ProjectSettingsDto>> GetSettings(Guid id, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(new GetProjectSettingsQuery(id, ownerId.Value), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message == "PROJECT_NOT_FOUND")
        {
            return NotFound(new ProblemDetails { Status = 404, Title = ex.Message });
        }
    }

    [HttpPut("{id:guid}/settings")]
    public async Task<ActionResult<ProjectSettingsDto>> UpdateSettings(Guid id, [FromBody] UpdateProjectSettingsRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(new UpdateProjectSettingsCommand(id, ownerId.Value, request), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message == "PROJECT_NOT_FOUND")
        {
            return NotFound(new ProblemDetails { Status = 404, Title = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message == "PROJECT_INVALID_TYPE")
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = ex.Message });
        }
    }

    private Guid? GetOwnerId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public record CreateProjectRequest(
    string ProjectName,
    string ProjectType,
    string? City,
    string? LocationText,
    string? MapLink,
    string? CurrentStage,
    string? OptionalImageUrl);
