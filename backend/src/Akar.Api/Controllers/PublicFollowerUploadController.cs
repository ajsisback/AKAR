using Akar.Application.Features.UploadLinks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Akar.Api.Controllers;

/// <summary>
/// Public (no JWT) endpoints for follower file uploads via upload token.
/// </summary>
[ApiController]
[Route("api/public/follower-upload/{token}")]
public class PublicFollowerUploadController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicFollowerUploadController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get minimal info for the upload page. No sensitive data exposed.</summary>
    [HttpGet("info")]
    public async Task<IActionResult> Info(string token, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFollowerUploadInfoQuery(token), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Upload a file using a follower upload token. No JWT required.</summary>
    [HttpPost("files")]
    [RequestSizeLimit(105 * 1024 * 1024)] // 105 MB (100 MB video + overhead)
    public async Task<IActionResult> Upload(string token, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails { Status = 400, Title = "FILE_REQUIRED", Detail = "A file is required" });

        using var stream = file.OpenReadStream();

        var command = new UploadFollowerFileCommand(
            token, file.FileName, file.ContentType,
            file.Length, stream);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    private IActionResult MapError(string code, string detail) => code switch
    {
        "UPLOAD_LINK_NOT_FOUND" => NotFound(new ProblemDetails { Status = 404, Title = code, Detail = detail }),
        "UPLOAD_LINK_REVOKED" => BadRequest(new ProblemDetails { Status = 400, Title = code, Detail = detail }),
        "UPLOAD_LINK_EXPIRED" => BadRequest(new ProblemDetails { Status = 400, Title = code, Detail = detail }),
        "FOLLOWER_NOT_FOUND" or "FOLLOWER_INACTIVE" => NotFound(new ProblemDetails { Status = 404, Title = code, Detail = detail }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = code, Detail = detail })
    };
}
