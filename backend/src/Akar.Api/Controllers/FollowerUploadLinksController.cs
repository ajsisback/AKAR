using Akar.Application.Features.UploadLinks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Akar.Api.Controllers;

/// <summary>
/// Owner-authenticated endpoints for managing follower upload links.
/// </summary>
[ApiController]
[Route("api/projects/{projectId:guid}/followers/{followerId:guid}")]
[Authorize]
public class FollowerUploadLinksController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowerUploadLinksController(IMediator mediator) => _mediator = mediator;

    /// <summary>Generate a new upload link for a follower. Raw token is returned once only.</summary>
    [HttpPost("upload-link")]
    public async Task<IActionResult> Generate(
        Guid projectId, Guid followerId,
        [FromBody] GenerateUploadLinkRequest? request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new GenerateFollowerUploadLinkCommand(
            projectId, followerId, ownerId.Value, request?.ExpiresAtUtc);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(List), new { projectId, followerId }, result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>List all upload links for a follower. No raw tokens returned.</summary>
    [HttpGet("upload-links")]
    public async Task<IActionResult> List(
        Guid projectId, Guid followerId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new ListFollowerUploadLinksQuery(projectId, followerId, ownerId.Value), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Revoke an upload link.</summary>
    [HttpPost("upload-links/{linkId:guid}/revoke")]
    public async Task<IActionResult> Revoke(
        Guid projectId, Guid followerId, Guid linkId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new RevokeFollowerUploadLinkCommand(projectId, followerId, linkId, ownerId.Value), cancellationToken);

        return result.IsSuccess ? NoContent() : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    private Guid? GetOwnerId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private IActionResult MapError(string code, string detail) => code switch
    {
        "PROJECT_NOT_FOUND" or "FOLLOWER_NOT_FOUND" or "UPLOAD_LINK_NOT_FOUND"
            => NotFound(new ProblemDetails { Status = 404, Title = code, Detail = detail }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = code, Detail = detail })
    };
}

public record GenerateUploadLinkRequest(DateTime? ExpiresAtUtc);
