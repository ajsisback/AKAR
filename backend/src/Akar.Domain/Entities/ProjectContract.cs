using Akar.Domain.Common;
using Akar.Domain.Enums;

namespace Akar.Domain.Entities;

/// <summary>
/// A contract created by the owner for a specific project, based on a template.
/// ContractDataJson stores structured data for future PDF generation.
/// Only Draft contracts can be updated in Sprint 4A.
/// </summary>
public class ProjectContract : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid ContractTemplateId { get; private set; }
    public string? ContractNumber { get; private set; }
    public string ContractTitle { get; private set; } = string.Empty;
    public ContractType ContractType { get; private set; }
    public string PartyName { get; private set; } = string.Empty;
    public string? PartyPhone { get; private set; }
    public string? PartyNationalId { get; private set; }
    public decimal? ContractValue { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string ContractDataJson { get; private set; } = "{}";
    public ContractStatus Status { get; private set; }
    public Guid? PdfFileId { get; private set; }
    public Guid? SignedFileId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    // Navigation
    public Project Project { get; private set; } = null!;
    public Owner Owner { get; private set; } = null!;
    public ContractTemplate ContractTemplate { get; private set; } = null!;
    public ProjectFile? PdfFile { get; private set; }
    public ProjectFile? SignedFile { get; private set; }

    private ProjectContract() { } // EF Core

    public static ProjectContract Create(
        Guid projectId,
        Guid ownerId,
        Guid contractTemplateId,
        ContractType contractType,
        string contractTitle,
        string partyName,
        string? partyPhone,
        string? partyNationalId,
        decimal? contractValue,
        DateTime? startDate,
        DateTime? endDate,
        string contractDataJson)
    {
        return new ProjectContract(Guid.NewGuid())
        {
            ProjectId = projectId,
            OwnerId = ownerId,
            ContractTemplateId = contractTemplateId,
            ContractType = contractType,
            ContractTitle = contractTitle.Trim(),
            PartyName = partyName.Trim(),
            PartyPhone = partyPhone?.Trim(),
            PartyNationalId = partyNationalId?.Trim(),
            ContractValue = contractValue,
            StartDate = ToUtc(startDate),
            EndDate = ToUtc(endDate),
            ContractDataJson = contractDataJson,
            Status = ContractStatus.Draft,
            IsDeleted = false
        };
    }

    /// <summary>Updates draft contract data. Returns false if not Draft or deleted.</summary>
    public bool UpdateDraft(
        string contractTitle,
        string partyName,
        string? partyPhone,
        string? partyNationalId,
        decimal? contractValue,
        DateTime? startDate,
        DateTime? endDate,
        string contractDataJson)
    {
        if (IsDeleted || Status != ContractStatus.Draft) return false;
        ContractTitle = contractTitle.Trim();
        PartyName = partyName.Trim();
        PartyPhone = partyPhone?.Trim();
        PartyNationalId = partyNationalId?.Trim();
        ContractValue = contractValue;
        StartDate = ToUtc(startDate);
        EndDate = ToUtc(endDate);
        ContractDataJson = contractDataJson;
        SetUpdatedAt();
        return true;
    }

    /// <summary>Soft-deletes the contract.</summary>
    public bool SoftDelete()
    {
        if (IsDeleted) return false;
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        Status = ContractStatus.Cancelled;
        SetUpdatedAt();
        return true;
    }

    private ProjectContract(Guid id) : base(id) { }

    private static DateTime? ToUtc(DateTime? dt) =>
        dt.HasValue && dt.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc)
            : dt;
}
