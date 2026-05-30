using Akar.Application.Features.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/contract-templates")]
[Authorize]
public class ContractTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractTemplatesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists all active contract templates.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListContractTemplatesQuery(), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new ProblemDetails { Status = 400, Title = result.ErrorCode!, Detail = result.ErrorMessage! });
    }

    /// <summary>Gets a contract template by ID.</summary>
    [HttpGet("{templateId:guid}")]
    public async Task<IActionResult> Get(Guid templateId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetContractTemplateQuery(templateId), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new ProblemDetails { Status = 404, Title = result.ErrorCode!, Detail = result.ErrorMessage! });
    }
}
