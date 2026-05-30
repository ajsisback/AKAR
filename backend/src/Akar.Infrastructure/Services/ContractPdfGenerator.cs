using System.Text.Json;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Akar.Infrastructure.Services;

/// <summary>
/// Generates Arabic-first contract PDFs using QuestPDF.
/// Content direction is RTL; English secondary labels included inline.
/// </summary>
public class ContractPdfGenerator : IContractPdfGenerator
{
    public byte[] Generate(ProjectContract contract, Project project, Owner owner)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(40);
                page.MarginVertical(30);
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(h => ComposeHeader(h, contract));
                page.Content().Element(c => ComposeBody(c, contract, project, owner));
                page.Footer().Element(ComposeFooter);
            });
        });

        return doc.GeneratePdf();
    }

    // ───────────────────── Header ─────────────────────

    private static void ComposeHeader(IContainer container, ProjectContract contract)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text("AKAR").Bold().FontSize(22);
            col.Item().AlignCenter().Text("أكار — منصة المالك").FontSize(10).FontColor(Colors.Grey.Darken1);
            col.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            col.Item().AlignCenter().Text(contract.ContractTitle).Bold().FontSize(16);
            col.Item().AlignCenter().Text($"{ContractTypeAr(contract.ContractType)} / {contract.ContractType}")
                .FontSize(10).FontColor(Colors.Grey.Darken2);

            if (!string.IsNullOrWhiteSpace(contract.ContractNumber))
                col.Item().AlignCenter().Text($"رقم العقد: {contract.ContractNumber}").FontSize(9);

            col.Item().AlignCenter().Text($"تاريخ الإنشاء: {DateTime.UtcNow:yyyy-MM-dd}").FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().PaddingBottom(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
        });
    }

    // ───────────────────── Body ─────────────────────

    private static void ComposeBody(IContainer container, ProjectContract contract, Project project, Owner owner)
    {
        container.PaddingVertical(4).Column(col =>
        {
            col.Spacing(6);

            // 1 — Project Information
            SectionTitle(col, "معلومات المشروع", "Project Information");
            InfoRow(col, "اسم المشروع", project.ProjectName);
            InfoRow(col, "نوع المشروع", project.ProjectType.ToString());
            if (!string.IsNullOrWhiteSpace(project.City))
                InfoRow(col, "المدينة", project.City);
            if (!string.IsNullOrWhiteSpace(project.LocationText))
                InfoRow(col, "الموقع", project.LocationText);

            // 2 — Owner Information
            SectionTitle(col, "معلومات المالك", "Owner Information");
            InfoRow(col, "اسم المالك", owner.FullName);
            if (!string.IsNullOrWhiteSpace(owner.Phone))
                InfoRow(col, "الهاتف", owner.Phone);
            if (!string.IsNullOrWhiteSpace(owner.Email))
                InfoRow(col, "البريد الإلكتروني", owner.Email);

            // 3 — Other Party Information
            SectionTitle(col, "معلومات الطرف الآخر", "Other Party Information");
            InfoRow(col, "الاسم", contract.PartyName);
            if (!string.IsNullOrWhiteSpace(contract.PartyPhone))
                InfoRow(col, "الهاتف", contract.PartyPhone);
            if (!string.IsNullOrWhiteSpace(contract.PartyNationalId))
                InfoRow(col, "رقم الهوية", contract.PartyNationalId);

            // 4 — Contract Values & Dates
            SectionTitle(col, "القيم والتواريخ", "Values & Dates");
            if (contract.ContractValue.HasValue)
                InfoRow(col, "قيمة العقد", $"{contract.ContractValue:N2} ريال");
            if (contract.StartDate.HasValue)
                InfoRow(col, "تاريخ البداية", contract.StartDate.Value.ToString("yyyy-MM-dd"));
            if (contract.EndDate.HasValue)
                InfoRow(col, "تاريخ الانتهاء", contract.EndDate.Value.ToString("yyyy-MM-dd"));

            // 5 — Contract Data (structured JSON fields)
            ComposeContractData(col, contract.ContractDataJson);

            // 6 — Default Terms from template
            ComposeDefaultTerms(col, contract.ContractTemplate?.DefaultTermsJson);

            // 7 — Disclaimer
            ComposeDisclaimer(col);

            // 8 — Signature Section
            ComposeSignatures(col, owner.FullName, contract.PartyName);
        });
    }

    // ───────────────────── Contract Data ─────────────────────

    private static void ComposeContractData(ColumnDescriptor col, string? contractDataJson)
    {
        if (string.IsNullOrWhiteSpace(contractDataJson) || contractDataJson == "{}") return;

        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(contractDataJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (data is null || data.Count == 0) return;

            SectionTitle(col, "بيانات العقد", "Contract Data");

            var fieldLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "scopeOfWork", "نطاق العمل" },
                { "paymentTerms", "شروط الدفع" },
                { "ownerObligations", "التزامات المالك" },
                { "contractorObligations", "التزامات المقاول" },
                { "notes", "ملاحظات" }
            };

            foreach (var (key, element) in data)
            {
                var label = fieldLabels.GetValueOrDefault(key, key);
                var value = element.ValueKind == JsonValueKind.String
                    ? element.GetString() ?? ""
                    : element.ToString();

                if (!string.IsNullOrWhiteSpace(value))
                    LongInfoRow(col, label, value);
            }
        }
        catch
        {
            // Invalid JSON — skip gracefully
        }
    }

    // ───────────────────── Default Terms ─────────────────────

    private static void ComposeDefaultTerms(ColumnDescriptor col, string? defaultTermsJson)
    {
        if (string.IsNullOrWhiteSpace(defaultTermsJson) || defaultTermsJson == "{}") return;

        try
        {
            var terms = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(defaultTermsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (terms is null || terms.Count == 0) return;

            SectionTitle(col, "الشروط الافتراضية", "Default Terms");

            foreach (var (key, element) in terms)
            {
                var value = element.ValueKind == JsonValueKind.String
                    ? element.GetString() ?? ""
                    : element.ToString();

                if (!string.IsNullOrWhiteSpace(value))
                    LongInfoRow(col, key, value);
            }
        }
        catch
        {
            // Invalid JSON — skip gracefully
        }
    }

    // ───────────────────── Disclaimer ─────────────────────

    private static void ComposeDisclaimer(ColumnDescriptor col)
    {
        col.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
        col.Item().PaddingVertical(6).Background(Colors.Grey.Lighten4).Padding(8).Column(inner =>
        {
            inner.Item().Text("إخلاء مسؤولية / Disclaimer").Bold().FontSize(10);
            inner.Item().PaddingTop(4).Text(
                "هذا النموذج مخصص لتنظيم العلاقة بين الأطراف ولا يغني عن المراجعة القانونية عند الحاجة.")
                .FontSize(9);
            inner.Item().PaddingTop(2).Text(
                "This template is intended to help organize the relationship between the parties and does not replace legal review when needed.")
                .FontSize(8).FontColor(Colors.Grey.Darken2);
        });
    }

    // ───────────────────── Signatures ─────────────────────

    private static void ComposeSignatures(ColumnDescriptor col, string ownerName, string partyName)
    {
        col.Item().PaddingTop(20).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("توقيع المالك / Owner Signature").Bold().FontSize(10);
                c.Item().PaddingTop(4).Text($"الاسم: {ownerName}").FontSize(9);
                c.Item().PaddingTop(30).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                c.Item().Text("التوقيع").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.ConstantItem(40); // spacer

            row.RelativeItem().Column(c =>
            {
                c.Item().Text("توقيع الطرف الآخر / Other Party Signature").Bold().FontSize(10);
                c.Item().PaddingTop(4).Text($"الاسم: {partyName}").FontSize(9);
                c.Item().PaddingTop(30).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                c.Item().Text("التوقيع").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });

        col.Item().PaddingTop(12).Row(row =>
        {
            row.RelativeItem().Text($"التاريخ: ________________").FontSize(9);
            row.ConstantItem(40);
            row.RelativeItem().Text($"التاريخ: ________________").FontSize(9);
        });
    }

    // ───────────────────── Footer ─────────────────────

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("AKAR — ").FontSize(8).FontColor(Colors.Grey.Darken1);
            text.CurrentPageNumber().FontSize(8);
            text.Span(" / ").FontSize(8);
            text.TotalPages().FontSize(8);
        });
    }

    // ───────────────────── Helpers ─────────────────────

    private static void SectionTitle(ColumnDescriptor col, string titleAr, string titleEn)
    {
        col.Item().PaddingTop(8).PaddingBottom(2).Text($"{titleAr}  —  {titleEn}")
            .Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
        col.Item().LineHorizontal(0.5f).LineColor(Colors.Blue.Lighten3);
    }

    private static void InfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().PaddingVertical(1).Row(row =>
        {
            row.ConstantItem(130).Text($"{label}:").Bold().FontSize(10);
            row.RelativeItem().Text(value).FontSize(10);
        });
    }

    private static void LongInfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().PaddingTop(3).Text($"{label}:").Bold().FontSize(10);
        col.Item().PaddingBottom(2).PaddingHorizontal(10).Text(value).FontSize(10);
    }

    private static string ContractTypeAr(Domain.Enums.ContractType type) => type switch
    {
        Domain.Enums.ContractType.StructuralContractor => "عقد مقاول عظم",
        Domain.Enums.ContractType.Electrician => "عقد كهربائي",
        Domain.Enums.ContractType.Plumber => "عقد سباك",
        Domain.Enums.ContractType.Supervisor => "عقد مشرف",
        Domain.Enums.ContractType.Designer => "عقد مصمم",
        Domain.Enums.ContractType.FinishingContractor => "عقد تشطيب عام",
        Domain.Enums.ContractType.GeneralCustom => "عقد مخصص",
        _ => "عقد"
    };
}
