namespace Akar.Application.DTOs;

public record ProjectContractDto(
    Guid Id,
    Guid ProjectId,
    Guid ContractTemplateId,
    string? ContractNumber,
    string ContractTitle,
    string ContractType,
    string PartyName,
    string? PartyPhone,
    string? PartyNationalId,
    decimal? ContractValue,
    DateTime? StartDate,
    DateTime? EndDate,
    string ContractDataJson,
    string Status,
    Guid? PdfFileId,
    Guid? SignedFileId,
    string? TemplateNameAr,
    string? TemplateNameEn,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
