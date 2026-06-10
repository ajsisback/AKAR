using Akar.Application.Features.Admin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    [HttpGet("owners")]
    public async Task<IActionResult> ListOwners(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListOwnersForAdminQuery(), cancellationToken);

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

    [HttpGet("owners/{ownerId:guid}")]
    public async Task<IActionResult> GetOwner(Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOwnerForAdminQuery(ownerId), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "OWNER_NOT_FOUND"
                ? NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage })
                : BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpGet("projects")]
    public async Task<IActionResult> ListProjects(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListProjectsForAdminQuery(), cancellationToken);

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

    [HttpGet("projects/{projectId:guid}")]
    public async Task<IActionResult> GetProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectForAdminQuery(projectId), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "PROJECT_NOT_FOUND"
                ? NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage })
                : BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage });
        }

        return Ok(result.Value);
    }
}
