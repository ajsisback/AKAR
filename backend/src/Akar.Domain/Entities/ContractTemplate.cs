using Akar.Domain.Common;
using Akar.Domain.Enums;

namespace Akar.Domain.Entities;

/// <summary>
/// A system-defined contract template. Owners cannot edit templates.
/// Contains field definitions and default terms as JSON for future UI/PDF use.
/// </summary>
public class ContractTemplate : Entity<Guid>
{
    public string TemplateCode { get; private set; } = string.Empty;
    public string TemplateNameAr { get; private set; } = string.Empty;
    public string TemplateNameEn { get; private set; } = string.Empty;
    public ContractType ContractType { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string DefaultTermsJson { get; private set; } = "{}";
    public string RequiredFieldsJson { get; private set; } = "[]";
    public bool IsActive { get; private set; }

    private ContractTemplate() { } // EF Core

    public static ContractTemplate Create(
        string templateCode,
        string templateNameAr,
        string templateNameEn,
        ContractType contractType,
        string? descriptionAr,
        string? descriptionEn,
        string defaultTermsJson,
        string requiredFieldsJson)
    {
        return new ContractTemplate(Guid.NewGuid())
        {
            TemplateCode = templateCode,
            TemplateNameAr = templateNameAr,
            TemplateNameEn = templateNameEn,
            ContractType = contractType,
            DescriptionAr = descriptionAr,
            DescriptionEn = descriptionEn,
            DefaultTermsJson = defaultTermsJson,
            RequiredFieldsJson = requiredFieldsJson,
            IsActive = true
        };
    }

    private ContractTemplate(Guid id) : base(id) { }
}
