using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRSystem.API.Services.Pdf;

public class PdfTemplateRenderer : IPdfTemplateRenderer
{
    public byte[] RenderPayslip(PayslipPdfModel model)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page);
                Header(page, model.CompanyName, "Payslip", model.GeneratedAtUtc);
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text(t =>
                    {
                        t.Span("Employee: ").SemiBold();
                        t.Span(model.EmployeeName);
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Period: ").SemiBold();
                        t.Span($"{model.Year}-{model.Month:00}");
                    });
                    if (!string.IsNullOrEmpty(model.JobTitle))
                        col.Item().Text(t => { t.Span("Title: ").SemiBold(); t.Span(model.JobTitle); });
                    if (!string.IsNullOrEmpty(model.DepartmentName))
                        col.Item().Text(t => { t.Span("Department: ").SemiBold(); t.Span(model.DepartmentName); });

                    col.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(); });
                        AddRow(table, "Base Salary", model.BaseSalary);
                        AddRow(table, "Allowances", model.Allowances);
                        AddRow(table, "Bonuses", model.Bonuses);
                        table.Cell().Element(CellBoldLabel).Text("Gross");
                        table.Cell().Element(CellBoldAmount).Text($"{model.Gross:F2}");
                        AddRow(table, "Deductions", -model.Deductions);
                        table.Cell().Element(CellBoldLabel).Text("Net");
                        table.Cell().Element(CellBoldAmount).Text($"{model.Net:F2}");
                    });
                });
                Footer(page);
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] RenderEmploymentLetter(EmploymentLetterModel model)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page);
                Header(page, model.CompanyName, "Employment Verification Letter", model.GeneratedAtUtc);
                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    col.Item().Text("To whom it may concern,").FontSize(11);
                    col.Item().Text(t =>
                    {
                        t.Span("This letter confirms that ");
                        t.Span(model.EmployeeName).SemiBold();
                        t.Span(" is currently employed at ");
                        t.Span(model.CompanyName).SemiBold();
                        t.Span(".");
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Position: ").SemiBold();
                        t.Span(model.JobTitle ?? "Not specified");
                    });
                    if (!string.IsNullOrEmpty(model.DepartmentName))
                        col.Item().Text(t => { t.Span("Department: ").SemiBold(); t.Span(model.DepartmentName); });
                    col.Item().Text(t =>
                    {
                        t.Span("Hire Date: ").SemiBold();
                        t.Span(model.HireDate.ToString("yyyy-MM-dd"));
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Employment Status: ").SemiBold();
                        t.Span(model.EmploymentStatus);
                    });
                    col.Item().PaddingTop(20).Text("Sincerely,").FontSize(11);
                    col.Item().Text("Human Resources Department").FontSize(11);
                });
                Footer(page);
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] RenderSalaryCertificate(SalaryCertificateModel model)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page);
                Header(page, model.CompanyName, "Salary Certificate", model.GeneratedAtUtc);
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text(t => { t.Span("Employee: ").SemiBold(); t.Span(model.EmployeeName); });
                    if (!string.IsNullOrEmpty(model.JobTitle))
                        col.Item().Text(t => { t.Span("Position: ").SemiBold(); t.Span(model.JobTitle); });
                    col.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(); });
                        AddRow(table, $"Base Salary ({model.Currency})", model.BaseSalary);
                        foreach (var allowance in model.Allowances)
                            AddRow(table, allowance.Type, allowance.Amount);
                        table.Cell().Element(CellBoldLabel).Text($"Total Compensation ({model.Currency})");
                        table.Cell().Element(CellBoldAmount).Text($"{model.TotalCompensation:F2}");
                    });
                });
                Footer(page);
            });
        });
        return doc.GeneratePdf();
    }

    // ===== Shared layout helpers =====

    private static void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(40);
        page.DefaultTextStyle(t => t.FontSize(10));
    }

    private static void Header(PageDescriptor page, string company, string docType, DateTime generatedAt)
    {
        page.Header().Row(row =>
        {
            row.RelativeItem().Text(company).FontSize(14).SemiBold();
            row.RelativeItem().AlignRight().Text(t =>
            {
                t.Span(docType).FontSize(12).SemiBold();
                t.Span($"\nGenerated: {generatedAt:yyyy-MM-dd HH:mm} UTC").FontSize(9);
            });
        });
    }

    private static void Footer(PageDescriptor page)
    {
        page.Footer().AlignCenter().Text(t =>
        {
            t.Span("Page ").FontSize(8);
            t.CurrentPageNumber().FontSize(8);
            t.Span(" / ").FontSize(8);
            t.TotalPages().FontSize(8);
            t.Span(" — Generated by HR System").FontSize(8);
        });
    }

    private static void AddRow(TableDescriptor table, string label, decimal amount)
    {
        table.Cell().Element(CellLabel).Text(label);
        table.Cell().Element(CellAmount).Text($"{amount:F2}");
    }

    private static IContainer CellLabel(IContainer c) => c.PaddingVertical(2);
    private static IContainer CellAmount(IContainer c) => c.PaddingVertical(2).AlignRight();
    private static IContainer CellBoldLabel(IContainer c) => c.PaddingVertical(2).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten1);
    private static IContainer CellBoldAmount(IContainer c) => c.PaddingVertical(2).AlignRight().BorderTop(0.5f).BorderColor(Colors.Grey.Lighten1);
}
