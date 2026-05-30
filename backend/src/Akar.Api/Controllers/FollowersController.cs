using Akar.Application.Features.Followers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/followers")]
[Authorize]
public class FollowersController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists all active followers for a project.</summary>
    [HttpGet]
    public async Task<IActionResult> List(Guid projectId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new ListProjectFollowersQuery(projectId, ownerId.Value), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Creates a follower and their dedicated inbox folder.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        Guid projectId,
        [FromBody] CreateFollowerRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new CreateProjectFollowerCommand(
            projectId, ownerId.Value,
            request.FullName, request.Phone,
            request.FollowerType, request.Notes);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { projectId, followerId = result.Value!.Id }, result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Gets a follower's details.</summary>
    [HttpGet("{followerId:guid}")]
    public async Task<IActionResult> Get(Guid projectId, Guid followerId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new GetProjectFollowerQuery(projectId, followerId, ownerId.Value), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Updates a follower's details. Renames inbox folder if name changes.</summary>
    [HttpPut("{followerId:guid}")]
    public async Task<IActionResult> Update(
        Guid projectId,
        Guid followerId,
        [FromBody] UpdateFollowerRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new UpdateProjectFollowerCommand(
            projectId, followerId, ownerId.Value,
            request.FullName, request.Phone,
            request.FollowerType, request.Notes,
            request.IsActive);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Soft-deletes a follower. Inbox folder and files are preserved.</summary>
    [HttpDelete("{followerId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid followerId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new DeleteProjectFollowerCommand(projectId, followerId, ownerId.Value), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    private Guid? GetOwnerId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private IActionResult MapError(string code, string detail) => code switch
    {
        "PROJECT_NOT_FOUND" or "FOLLOWER_NOT_FOUND" or "FOLLOWERS_INBOX_NOT_FOUND"
            => NotFound(new ProblemDetails { Status = 404, Title = code, Detail = detail }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = code, Detail = detail })
    };
}

public record CreateFollowerRequest(string FullName, string Phone, string FollowerType, string? Notes);
public record UpdateFollowerRequest(string FullName, string Phone, string FollowerType, string? Notes, bool IsActive);
