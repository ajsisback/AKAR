using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Contracts;

// ═══════════════════════════════════════════════════════════════
// List active project contracts
// ═══════════════════════════════════════════════════════════════

public record ListProjectContractsQuery(
    Guid ProjectId, Guid OwnerId) : IRequest<Result<List<ProjectContractDto>>>;

public class ListProjectContractsQueryHandler
    : IRequestHandler<ListProjectContractsQuery, Result<List<ProjectContractDto>>>
{
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectContractRepository _repo;

    public ListProjectContractsQueryHandler(
        IProjectRepository projectRepo,
        IProjectContractRepository repo)
    {
        _projectRepo = projectRepo;
        _repo = repo;
    }

    public async Task<Result<List<ProjectContractDto>>> Handle(
        ListProjectContractsQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepo.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<List<ProjectContractDto>>.Failure("PROJECT_NOT_FOUND", "Project not found");

        var contracts = await _repo.GetActiveByProjectForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);

        var dtos = contracts.Select(MapToDto).ToList();
        return Result<List<ProjectContractDto>>.Success(dtos);
    }

    public static ProjectContractDto MapToDto(ProjectContract c) => new(
        c.Id, c.ProjectId, c.ContractTemplateId,
        c.ContractNumber, c.ContractTitle, c.ContractType.ToString(),
        c.PartyName, c.PartyPhone, c.PartyNationalId,
        c.ContractValue, c.StartDate, c.EndDate,
        c.ContractDataJson, c.Status.ToString(),
        c.PdfFileId, c.SignedFileId,
        c.ContractTemplate?.TemplateNameAr,
        c.ContractTemplate?.TemplateNameEn,
        c.CreatedAtUtc, c.UpdatedAtUtc);
}

// ═══════════════════════════════════════════════════════════════
// Get project contract by ID
// ═══════════════════════════════════════════════════════════════

public record GetProjectContractQuery(
    Guid ProjectId, Guid ContractId, Guid OwnerId) : IRequest<Result<ProjectContractDto>>;

public class GetProjectContractQueryHandler
    : IRequestHandler<GetProjectContractQuery, Result<ProjectContractDto>>
{
    private readonly IProjectContractRepository _repo;

    public GetProjectContractQueryHandler(IProjectContractRepository repo) => _repo = repo;

    public async Task<Result<ProjectContractDto>> Handle(
        GetProjectContractQuery request, CancellationToken cancellationToken)
    {
        var contract = await _repo.GetByIdForOwnerAsync(request.ContractId, request.OwnerId, cancellationToken);
        if (contract is null || contract.ProjectId != request.ProjectId)
            return Result<ProjectContractDto>.Failure("CONTRACT_NOT_FOUND", "Contract not found");

        return Result<ProjectContractDto>.Success(
            ListProjectContractsQueryHandler.MapToDto(contract));
    }
}

// ═══════════════════════════════════════════════════════════════
// Create project contract from template
// ═══════════════════════════════════════════════════════════════

public record CreateProjectContractCommand(
    Guid ProjectId,
    Guid OwnerId,
    Guid ContractTemplateId,
    string ContractTitle,
    string PartyName,
    string? PartyPhone,
    string? PartyNationalId,
    decimal? ContractValue,
    DateTime? StartDate,
    DateTime? EndDate,
    string ContractDataJson) : IRequest<Result<ProjectContractDto>>;

public class CreateProjectContractCommandHandler
    : IRequestHandler<CreateProjectContractCommand, Result<ProjectContractDto>>
{
    private readonly IProjectRepository _projectRepo;
    private readonly IContractTemplateRepository _templateRepo;
    private readonly IProjectContractRepository _contractRepo;
    private readonly IProjectTimelineEventWriter _timelineWriter;

    public CreateProjectContractCommandHandler(
        IProjectRepository projectRepo,
        IContractTemplateRepository templateRepo,
        IProjectContractRepository contractRepo,
        IProjectTimelineEventWriter timelineWriter)
    {
        _projectRepo = projectRepo;
        _templateRepo = templateRepo;
        _contractRepo = contractRepo;
        _timelineWriter = timelineWriter;
    }

    public async Task<Result<ProjectContractDto>> Handle(
        CreateProjectContractCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate project ownership
        var project = await _projectRepo.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<ProjectContractDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        // 2. Validate template
        var template = await _templateRepo.GetByIdAsync(request.ContractTemplateId, cancellationToken);
        if (template is null || !template.IsActive)
            return Result<ProjectContractDto>.Failure("CONTRACT_TEMPLATE_NOT_FOUND", "Contract template not found");

        // 3. Validate title
        if (string.IsNullOrWhiteSpace(request.ContractTitle))
            return Result<ProjectContractDto>.Failure("CONTRACT_TITLE_REQUIRED", "Contract title is required");

        // 4. Validate party
        if (string.IsNullOrWhiteSpace(request.PartyName))
            return Result<ProjectContractDto>.Failure("CONTRACT_PARTY_REQUIRED", "Party name is required");

        // 5. Validate dates
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
            return Result<ProjectContractDto>.Failure("INVALID_CONTRACT_DATES", "End date must be after start date");

        // 6. Create
        var contract = ProjectContract.Create(
            request.ProjectId,
            request.OwnerId,
            template.Id,
            template.ContractType,
            request.ContractTitle,
            request.PartyName,
            request.PartyPhone,
            request.PartyNationalId,
            request.ContractValue,
            request.StartDate,
            request.EndDate,
            string.IsNullOrWhiteSpace(request.ContractDataJson) ? "{}" : request.ContractDataJson);

        await _contractRepo.AddAsync(contract, cancellationToken);
        await _contractRepo.SaveChangesAsync(cancellationToken);

        // 7. Create timeline event
        var description = !string.IsNullOrWhiteSpace(request.PartyName)
            ? $"{request.ContractTitle} - {request.PartyName}"
            : request.ContractTitle;
        await _timelineWriter.AddSystemEventAsync(
            project.Id, project.OwnerId, project.CurrentStage,
            TimelineEventType.ContractCreated, TimelineSourceType.ProjectContract, contract.Id,
            "Contract created", description,
            cancellationToken);

        return Result<ProjectContractDto>.Success(
            ListProjectContractsQueryHandler.MapToDto(contract));
    }
}

// ═══════════════════════════════════════════════════════════════
// Update draft project contract
// ═══════════════════════════════════════════════════════════════

public record UpdateProjectContractCommand(
    Guid ProjectId,
    Guid ContractId,
    Guid OwnerId,
    string ContractTitle,
    string PartyName,
    string? PartyPhone,
    string? PartyNationalId,
    decimal? ContractValue,
    DateTime? StartDate,
    DateTime? EndDate,
    string ContractDataJson) : IRequest<Result<ProjectContractDto>>;

public class UpdateProjectContractCommandHandler
    : IRequestHandler<UpdateProjectContractCommand, Result<ProjectContractDto>>
{
    private readonly IProjectContractRepository _repo;

    public UpdateProjectContractCommandHandler(IProjectContractRepository repo) => _repo = repo;

    public async Task<Result<ProjectContractDto>> Handle(
        UpdateProjectContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await _repo.GetByIdForOwnerAsync(request.ContractId, request.OwnerId, cancellationToken);
        if (contract is null || contract.ProjectId != request.ProjectId)
            return Result<ProjectContractDto>.Failure("CONTRACT_NOT_FOUND", "Contract not found");

        // Validate title
        if (string.IsNullOrWhiteSpace(request.ContractTitle))
            return Result<ProjectContractDto>.Failure("CONTRACT_TITLE_REQUIRED", "Contract title is required");

        // Validate party
        if (string.IsNullOrWhiteSpace(request.PartyName))
            return Result<ProjectContractDto>.Failure("CONTRACT_PARTY_REQUIRED", "Party name is required");

        // Validate dates
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
            return Result<ProjectContractDto>.Failure("INVALID_CONTRACT_DATES", "End date must be after start date");

        // Only Draft can be updated
        var updated = contract.UpdateDraft(
            request.ContractTitle,
            request.PartyName,
            request.PartyPhone,
            request.PartyNationalId,
            request.ContractValue,
            request.StartDate,
            request.EndDate,
            string.IsNullOrWhiteSpace(request.ContractDataJson) ? "{}" : request.ContractDataJson);

        if (!updated)
            return Result<ProjectContractDto>.Failure("CONTRACT_NOT_DRAFT", "Only draft contracts can be updated");

        await _repo.SaveChangesAsync(cancellationToken);

        return Result<ProjectContractDto>.Success(
            ListProjectContractsQueryHandler.MapToDto(contract));
    }
}

// ═══════════════════════════════════════════════════════════════
// Delete (soft-delete) project contract
// ═══════════════════════════════════════════════════════════════

public record DeleteProjectContractCommand(
    Guid ProjectId, Guid ContractId, Guid OwnerId) : IRequest<Result<bool>>;

public class DeleteProjectContractCommandHandler
    : IRequestHandler<DeleteProjectContractCommand, Result<bool>>
{
    private readonly IProjectContractRepository _repo;

    public DeleteProjectContractCommandHandler(IProjectContractRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(
        DeleteProjectContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await _repo.GetByIdForOwnerAsync(request.ContractId, request.OwnerId, cancellationToken);
        if (contract is null || contract.ProjectId != request.ProjectId)
            return Result<bool>.Failure("CONTRACT_NOT_FOUND", "Contract not found");

        contract.SoftDelete();
        await _repo.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
