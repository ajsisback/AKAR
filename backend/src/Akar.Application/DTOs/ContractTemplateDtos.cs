namespace Akar.Application.DTOs;

public record ContractTemplateDto(
    Guid Id,
    string TemplateCode,
    string TemplateNameAr,
    string TemplateNameEn,
    string ContractType,
    string? DescriptionAr,
    string? DescriptionEn,
    string DefaultTermsJson,
    string RequiredFieldsJson);
