using Akar.Application.DTOs;
using Akar.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterOwnerCommand(
            request.FullName,
            request.Email,
            request.Phone,
            request.Password,
            request.CompanyName);

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

        return Created(string.Empty, result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginOwnerCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = 401,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(result.Value);
    }
}
