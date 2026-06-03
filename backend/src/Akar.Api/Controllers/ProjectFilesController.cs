using Akar.Application.Features.Files;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Akar.Api.Controllers;

/// <summary>
/// Project-scoped file operations: metadata, download, soft-delete, restore, trash.
/// </summary>
[ApiController]
[Route("api/projects/{projectId:guid}")]
[Authorize]
public class ProjectFilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectFilesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Search and filter files across the entire project.</summary>
    [HttpGet("files/search")]
    public async Task<IActionResult> Search(
        Guid projectId,
        [FromQuery] string? q = null,
        [FromQuery] Guid? folderId = null,
        [FromQuery] string? fileCategory = null,
        [FromQuery] string? extension = null,
        [FromQuery] string? contentType = null,
        [FromQuery] DateTime? createdFromUtc = null,
        [FromQuery] DateTime? createdToUtc = null,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] string sortBy = "createdAtUtc",
        [FromQuery] string sortDirection = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var query = new SearchProjectFilesQuery(
            projectId, ownerId.Value, q, folderId, fileCategory, extension,
            contentType, createdFromUtc, createdToUtc, includeDeleted,
            sortBy, sortDirection, page, pageSize);

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "PROJECT_NOT_FOUND"
                    => NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                "INVALID_PAGE" or "INVALID_PAGE_SIZE" or "INVALID_SORT_BY" or
                "INVALID_SORT_DIRECTION" or "INVALID_DATE_RANGE" or "INVALID_FILE_CATEGORY"
                    => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                _ => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>Get file metadata.</summary>
    [HttpGet("files/{fileId:guid}")]
    public async Task<IActionResult> GetMetadata(Guid projectId, Guid fileId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(new GetProjectFileQuery(projectId, fileId, ownerId.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "PROJECT_NOT_FOUND" or "FILE_NOT_FOUND"
                    => NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                _ => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>Download a file. Returns file stream with original filename.</summary>
    [HttpGet("files/{fileId:guid}/download")]
    public async Task<IActionResult> Download(Guid projectId, Guid fileId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(new DownloadProjectFileQuery(projectId, fileId, ownerId.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "PROJECT_NOT_FOUND" or "FILE_NOT_FOUND"
                    => NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                _ => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage })
            };
        }

        var download = result.Value!;
        return File(download.Stream, download.ContentType, download.OriginalFileName);
    }

    /// <summary>Soft-delete a file. Does not physically remove the file.</summary>
    [HttpDelete("files/{fileId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid fileId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(new DeleteProjectFileCommand(projectId, fileId, ownerId.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "PROJECT_NOT_FOUND" or "FILE_NOT_FOUND"
                    => NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                _ => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage })
            };
        }

        return NoContent();
    }

    /// <summary>Restore a soft-deleted file.</summary>
    [HttpPost("files/{fileId:guid}/restore")]
    public async Task<IActionResult> Restore(Guid projectId, Guid fileId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(new RestoreProjectFileCommand(projectId, fileId, ownerId.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "PROJECT_NOT_FOUND" or "FILE_NOT_FOUND"
                    => NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                _ => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage })
            };
        }

        return Ok(new { message = "File restored successfully" });
    }

    /// <summary>List deleted files and deleted custom folders for a project.</summary>
    [HttpGet("trash")]
    public async Task<IActionResult> Trash(Guid projectId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(new GetProjectTrashQuery(projectId, ownerId.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "PROJECT_NOT_FOUND"
                    => NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                _ => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage })
            };
        }

        return Ok(result.Value);
    }

    private Guid? GetOwnerId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
