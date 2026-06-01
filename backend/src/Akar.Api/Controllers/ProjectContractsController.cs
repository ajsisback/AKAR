using Akar.Application.Features.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Akar.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/contracts")]
[Authorize]
public class ProjectContractsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectContractsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists active contracts for a project.</summary>
    [HttpGet]
    public async Task<IActionResult> List(Guid projectId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new ListProjectContractsQuery(projectId, ownerId.Value), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Creates a project contract from a template.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        Guid projectId,
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new CreateProjectContractCommand(
            projectId, ownerId.Value,
            request.ContractTemplateId,
            request.ContractTitle,
            request.PartyName,
            request.PartyPhone,
            request.PartyNationalId,
            request.ContractValue,
            request.StartDate,
            request.EndDate,
            request.ContractDataJson ?? "{}");

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { projectId, contractId = result.Value!.Id }, result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Gets a project contract by ID.</summary>
    [HttpGet("{contractId:guid}")]
    public async Task<IActionResult> Get(Guid projectId, Guid contractId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new GetProjectContractQuery(projectId, contractId, ownerId.Value), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Updates a draft project contract.</summary>
    [HttpPut("{contractId:guid}")]
    public async Task<IActionResult> Update(
        Guid projectId,
        Guid contractId,
        [FromBody] UpdateContractRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var command = new UpdateProjectContractCommand(
            projectId, contractId, ownerId.Value,
            request.ContractTitle,
            request.PartyName,
            request.PartyPhone,
            request.PartyNationalId,
            request.ContractValue,
            request.StartDate,
            request.EndDate,
            request.ContractDataJson ?? "{}");

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Soft-deletes a project contract.</summary>
    [HttpDelete("{contractId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid contractId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new DeleteProjectContractCommand(projectId, contractId, ownerId.Value), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Generates a PDF for the project contract and saves it to the Contracts folder.</summary>
    [HttpPost("{contractId:guid}/generate-pdf")]
    public async Task<IActionResult> GeneratePdf(Guid projectId, Guid contractId, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new GenerateProjectContractPdfCommand(projectId, contractId, ownerId.Value), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.ErrorCode!, result.ErrorMessage!);
    }

    /// <summary>Uploads a signed contract PDF file and links it to the contract.</summary>
    [HttpPost("{contractId:guid}/signed-file")]
    public async Task<IActionResult> UploadSignedFile(
        Guid projectId,
        Guid contractId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails { Status = 400, Title = "SIGNED_FILE_REQUIRED", Detail = "A signed contract PDF file is required" });

        var command = new UploadSignedContractFileCommand(
            projectId, contractId, ownerId.Value,
            file.FileName,
            file.ContentType,
            file.Length,
            file.OpenReadStream());

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
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
        "PROJECT_NOT_FOUND" or "CONTRACT_NOT_FOUND" or "CONTRACT_TEMPLATE_NOT_FOUND"
            or "CONTRACTS_FOLDER_NOT_FOUND"
            => NotFound(new ProblemDetails { Status = 404, Title = code, Detail = detail }),
        "CONTRACT_NOT_ELIGIBLE_FOR_PDF" or "CONTRACT_CANCELLED"
            or "CONTRACT_NOT_PDF_GENERATED" or "CONTRACT_ALREADY_SIGNED"
            => Conflict(new ProblemDetails { Status = 409, Title = code, Detail = detail }),
        "SIGNED_FILE_MUST_BE_PDF" or "SIGNED_FILE_TOO_LARGE" or "SIGNED_FILE_REQUIRED"
            => BadRequest(new ProblemDetails { Status = 400, Title = code, Detail = detail }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = code, Detail = detail })
    };
}

public record CreateContractRequest(
    Guid ContractTemplateId,
    string ContractTitle,
    string PartyName,
    string? PartyPhone,
    string? PartyNationalId,
    decimal? ContractValue,
    DateTime? StartDate,
    DateTime? EndDate,
    string? ContractDataJson);

public record UpdateContractRequest(
    string ContractTitle,
    string PartyName,
    string? PartyPhone,
    string? PartyNationalId,
    decimal? ContractValue,
    DateTime? StartDate,
    DateTime? EndDate,
    string? ContractDataJson);
