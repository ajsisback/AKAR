using Akar.Application.DTOs;
using Akar.Application.Features.OwnerProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/owner")]
[Authorize]
public class OwnerProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public OwnerProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid? GetOwnerId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<OwnerProfileDto>> GetProfile()
    {
        var ownerId = GetOwnerId();
        if (ownerId == null) return Unauthorized();

        var result = await _mediator.Send(new GetOwnerProfileQuery(ownerId.Value));
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<OwnerProfileDto>> UpdateProfile([FromBody] UpdateOwnerProfileRequest request)
    {
        var ownerId = GetOwnerId();
        if (ownerId == null) return Unauthorized();

        var result = await _mediator.Send(new UpdateOwnerProfileCommand(ownerId.Value, request.FullName, request.Phone));
        return Ok(result);
    }

    [HttpPut("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var ownerId = GetOwnerId();
        if (ownerId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new ChangeOwnerPasswordCommand(
                ownerId.Value,
                request.CurrentPassword, 
                request.NewPassword, 
                request.ConfirmNewPassword));
            return NoContent();
        }
        catch (InvalidOperationException ex) when (
            ex.Message is "CURRENT_PASSWORD_INVALID" 
                       or "PASSWORD_SAME_AS_CURRENT" 
                       or "PASSWORD_CONFIRMATION_MISMATCH"
                       or "PASSWORD_TOO_WEAK")
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message == "OWNER_NOT_FOUND")
        {
            return NotFound(new ProblemDetails { Status = 404, Title = ex.Message });
        }
    }
}
