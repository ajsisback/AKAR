using Akar.Application.Features.Files;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Akar.Api.Controllers;

/// <summary>
/// File operations scoped to a specific folder within a project.
/// Handles upload and listing files by folder.
/// IFormFile stays in this API layer — the Application layer receives clean types only.
/// </summary>
[ApiController]
[Route("api/projects/{projectId:guid}/folders/{folderId:guid}/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Upload a file to a folder.</summary>
    [HttpPost]
    [RequestSizeLimit(110 * 1024 * 1024)] // 110 MB to accommodate video + overhead
    public async Task<IActionResult> Upload(Guid projectId, Guid folderId, IFormFile file, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "FILE_REQUIRED",
                Detail = "A file is required"
            });
        }

        // Extract clean types from IFormFile — Application layer never sees IFormFile
        await using var stream = file.OpenReadStream();
        var command = new UploadProjectFileCommand(
            projectId,
            folderId,
            ownerId.Value,
            file.FileName,
            file.ContentType,
            file.Length,
            stream);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "PROJECT_NOT_FOUND" or "FOLDER_NOT_FOUND" or "FILE_NOT_FOUND"
                    => NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode, Detail = result.ErrorMessage }),
                _ => BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode, Detail = result.ErrorMessage })
            };
        }

        return CreatedAtAction(
            actionName: "GetMetadata",
            controllerName: "ProjectFiles",
            routeValues: new { projectId, fileId = result.Value!.Id },
            value: result.Value);
    }

    /// <summary>List active files in a folder.</summary>
    [HttpGet]
    public async Task<IActionResult> List(Guid projectId, Guid folderId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(new GetFolderFilesQuery(projectId, folderId, ownerId.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "PROJECT_NOT_FOUND" or "FOLDER_NOT_FOUND"
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
