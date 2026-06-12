using Akar.Application.DTOs;
using Akar.Application.Features.AdminAuth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginAdminCommand(request.Email, request.Password);
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
