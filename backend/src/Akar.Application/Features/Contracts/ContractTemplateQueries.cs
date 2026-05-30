using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Contracts;

// ═══════════════════════════════════════════════════════════════
// List active contract templates
// ═══════════════════════════════════════════════════════════════

public record ListContractTemplatesQuery : IRequest<Result<List<ContractTemplateDto>>>;

public class ListContractTemplatesQueryHandler
    : IRequestHandler<ListContractTemplatesQuery, Result<List<ContractTemplateDto>>>
{
    private readonly IContractTemplateRepository _repo;

    public ListContractTemplatesQueryHandler(IContractTemplateRepository repo) => _repo = repo;

    public async Task<Result<List<ContractTemplateDto>>> Handle(
        ListContractTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _repo.GetAllActiveAsync(cancellationToken);
        var dtos = templates.Select(MapToDto).ToList();
        return Result<List<ContractTemplateDto>>.Success(dtos);
    }

    public static ContractTemplateDto MapToDto(ContractTemplate t) => new(
        t.Id, t.TemplateCode, t.TemplateNameAr, t.TemplateNameEn,
        t.ContractType.ToString(), t.DescriptionAr, t.DescriptionEn,
        t.DefaultTermsJson, t.RequiredFieldsJson);
}

// ═══════════════════════════════════════════════════════════════
// Get contract template by ID
// ═══════════════════════════════════════════════════════════════

public record GetContractTemplateQuery(Guid TemplateId) : IRequest<Result<ContractTemplateDto>>;

public class GetContractTemplateQueryHandler
    : IRequestHandler<GetContractTemplateQuery, Result<ContractTemplateDto>>
{
    private readonly IContractTemplateRepository _repo;

    public GetContractTemplateQueryHandler(IContractTemplateRepository repo) => _repo = repo;

    public async Task<Result<ContractTemplateDto>> Handle(
        GetContractTemplateQuery request, CancellationToken cancellationToken)
    {
        var template = await _repo.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null || !template.IsActive)
            return Result<ContractTemplateDto>.Failure("CONTRACT_TEMPLATE_NOT_FOUND", "Contract template not found");

        return Result<ContractTemplateDto>.Success(
            ListContractTemplatesQueryHandler.MapToDto(template));
    }
}
