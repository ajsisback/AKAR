using Akar.Application.Features.Folders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/folders")]
[Authorize]
public class FoldersController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoldersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists all active folders for a project.</summary>
    [HttpGet]
    public async Task<IActionResult> List(Guid projectId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(new ListFoldersQuery(projectId, ownerId.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "PROJECT_NOT_FOUND"
                ? NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage })
                : BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>Creates a custom folder within a project.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateFolderRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new CreateFolderCommand(
            projectId,
            ownerId.Value,
            request.FolderName,
            request.ParentFolderId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "PROJECT_NOT_FOUND"
                ? NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage })
                : BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(List), new { projectId }, result.Value);
    }

    /// <summary>Renames a custom folder. System folders cannot be renamed.</summary>
    [HttpPut("{folderId:guid}")]
    public async Task<IActionResult> Rename(Guid projectId, Guid folderId, [FromBody] RenameFolderRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new RenameFolderCommand(projectId, folderId, ownerId.Value, request.NewName);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "FOLDER_NOT_FOUND" => NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                "FOLDER_SYSTEM_PROTECTED" => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                _ => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>Soft-deletes a custom folder. System folders cannot be deleted.</summary>
    [HttpDelete("{folderId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid folderId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new DeleteFolderCommand(projectId, folderId, ownerId.Value);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "FOLDER_NOT_FOUND" => NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                "FOLDER_SYSTEM_PROTECTED" => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                _ => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage })
            };
        }

        return NoContent();
    }

    private Guid? GetOwnerId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public record CreateFolderRequest(string FolderName, Guid? ParentFolderId);
public record RenameFolderRequest(string NewName);
