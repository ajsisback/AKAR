using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Seed;

public static class ContractTemplateSeeder
{
    public static async Task SeedAsync(AkarDbContext db)
    {
        if (await db.ContractTemplates.AnyAsync())
            return; // Already seeded

        var requiredFields = """
        ["partyName","partyPhone","contractValue","startDate","endDate","scopeOfWork","paymentTerms","ownerObligations","contractorObligations","notes"]
        """.Trim();

        var templates = new List<ContractTemplate>
        {
            ContractTemplate.Create(
                "STRUCTURAL_CONTRACTOR",
                "عقد مقاول عظم",
                "Structural Contractor Contract",
                ContractType.StructuralContractor,
                "عقد تنفيذ أعمال الهيكل الإنشائي للمبنى السكني",
                "Contract for structural construction of residential building",
                """{"scopeOfWork":"أعمال الهيكل الإنشائي","paymentTerms":"دفعات حسب مراحل الإنجاز","ownerObligations":"توفير المخططات المعتمدة والتصاريح","contractorObligations":"تنفيذ الأعمال حسب المخططات والمواصفات","penaltyClause":"غرامة تأخير يومية","warrantyPeriod":"سنة واحدة"}""",
                requiredFields),

            ContractTemplate.Create(
                "ELECTRICIAN",
                "عقد كهربائي",
                "Electrician Contract",
                ContractType.Electrician,
                "عقد تنفيذ أعمال التمديدات الكهربائية",
                "Contract for electrical installation work",
                """{"scopeOfWork":"أعمال التمديدات الكهربائية","paymentTerms":"دفعات حسب مراحل الإنجاز","ownerObligations":"توفير المخططات الكهربائية المعتمدة","contractorObligations":"تنفيذ الأعمال حسب كود البناء السعودي","penaltyClause":"غرامة تأخير يومية","warrantyPeriod":"سنة واحدة"}""",
                requiredFields),

            ContractTemplate.Create(
                "PLUMBER",
                "عقد سباك",
                "Plumber Contract",
                ContractType.Plumber,
                "عقد تنفيذ أعمال السباكة والصرف الصحي",
                "Contract for plumbing and sanitation work",
                """{"scopeOfWork":"أعمال السباكة والصرف الصحي","paymentTerms":"دفعات حسب مراحل الإنجاز","ownerObligations":"توفير المخططات المعتمدة","contractorObligations":"تنفيذ الأعمال حسب المواصفات","penaltyClause":"غرامة تأخير يومية","warrantyPeriod":"سنة واحدة"}""",
                requiredFields),

            ContractTemplate.Create(
                "SUPERVISOR",
                "عقد مشرف",
                "Supervisor Contract",
                ContractType.Supervisor,
                "عقد إشراف على تنفيذ المشروع السكني",
                "Contract for residential project supervision",
                """{"scopeOfWork":"الإشراف على تنفيذ المشروع","paymentTerms":"راتب شهري أو دفعات دورية","ownerObligations":"توفير المخططات وصلاحية الدخول للموقع","contractorObligations":"متابعة التنفيذ وتقديم تقارير دورية","penaltyClause":"غرامة تأخير يومية","warrantyPeriod":"طوال فترة العقد"}""",
                requiredFields),

            ContractTemplate.Create(
                "DESIGNER",
                "عقد مصمم",
                "Designer Contract",
                ContractType.Designer,
                "عقد تصميم معماري أو داخلي للمشروع السكني",
                "Contract for architectural or interior design",
                """{"scopeOfWork":"التصميم المعماري أو الداخلي","paymentTerms":"دفعات حسب مراحل التصميم","ownerObligations":"توفير متطلبات التصميم والمساحات","contractorObligations":"تقديم التصاميم حسب المواصفات المتفق عليها","penaltyClause":"غرامة تأخير يومية","warrantyPeriod":"سنة واحدة"}""",
                requiredFields),

            ContractTemplate.Create(
                "FINISHING_CONTRACTOR",
                "عقد تشطيب عام",
                "General Finishing Contract",
                ContractType.FinishingContractor,
                "عقد تنفيذ أعمال التشطيب العام للمبنى السكني",
                "Contract for general finishing work",
                """{"scopeOfWork":"أعمال التشطيب العام","paymentTerms":"دفعات حسب مراحل الإنجاز","ownerObligations":"توفير المخططات والمواصفات المطلوبة","contractorObligations":"تنفيذ أعمال التشطيب حسب المواصفات","penaltyClause":"غرامة تأخير يومية","warrantyPeriod":"سنة واحدة"}""",
                requiredFields),

            ContractTemplate.Create(
                "GENERAL_CUSTOM",
                "عقد مخصص",
                "Custom Contract",
                ContractType.GeneralCustom,
                "عقد مخصص يمكن تعديله حسب الحاجة",
                "Custom contract that can be tailored as needed",
                """{"scopeOfWork":"","paymentTerms":"","ownerObligations":"","contractorObligations":"","penaltyClause":"","warrantyPeriod":""}""",
                requiredFields)
        };

        await db.ContractTemplates.AddRangeAsync(templates);
        await db.SaveChangesAsync();
    }
}
