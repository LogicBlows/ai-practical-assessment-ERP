using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;

namespace SmeErp.Infrastructure.Services;

public class QuotationPdfService : IQuotationPdfService
{
    private const string PdfFontFamily = "Arial";

    public byte[] GeneratePdf(QuotationDetailDto quotation, CompanySettingsDto companySettings)
    {
        var accentColor = ParseColor(companySettings.PrimaryColor);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(DefaultPdfTextStyle);

                page.Header().Element(header => ComposeHeader(header, companySettings, accentColor));
                page.Content().Element(content => ComposeContent(content, quotation, accentColor));
                page.Footer().Element(footer => ComposeFooter(footer, companySettings));
            });
        });

        return document.GeneratePdf();
    }

    private static TextStyle DefaultPdfTextStyle =>
        TextStyle.Default
            .FontFamily(PdfFontFamily)
            .FontSize(10)
            .DisableFontFeature(FontFeatures.StandardLigatures);

    private static void ComposeHeader(IContainer container, CompanySettingsDto company, Color accentColor)
    {
        container.Column(column =>
        {
            column.Item().Background(accentColor).Padding(12).Row(row =>
            {
                row.RelativeItem().Column(inner =>
                {
                    inner.Item().Text(company.CompanyName).FontSize(16).Bold().FontColor(Colors.White);
                    inner.Item().Text(company.Address).FontColor(Colors.White);
                    inner.Item().Text($"{company.City}, {company.State} {company.PinCode}, {company.Country}")
                        .FontColor(Colors.White);
                });

                row.ConstantItem(180).AlignRight().Column(inner =>
                {
                    inner.Item().Text($"GSTIN: {company.GstNumber}").FontColor(Colors.White);
                    inner.Item().Text($"PAN: {company.PanNumber}").FontColor(Colors.White);
                    inner.Item().Text($"Mobile: {company.Mobile}").FontColor(Colors.White);
                    inner.Item().Text($"Email: {company.Email}").FontColor(Colors.White);
                    if (!string.IsNullOrWhiteSpace(company.Website))
                    {
                        inner.Item().Text(company.Website).FontColor(Colors.White);
                    }
                });
            });
        });
    }

    private static void ComposeContent(IContainer container, QuotationDetailDto quotation, Color accentColor)
    {
        container.PaddingVertical(16).Column(column =>
        {
            column.Spacing(8);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text("Quotation").FontSize(14).Bold();
                    left.Item().Text($"No: {quotation.QuotationNumber}");
                    left.Item().Text($"Date: {quotation.QuotationDate:dd MMM yyyy}");
                    left.Item().Text($"Valid until: {quotation.ValidUntil:dd MMM yyyy}");
                });

                row.RelativeItem().Column(right =>
                {
                    right.Item().Text("Bill To").Bold();
                    right.Item().Text(quotation.CustomerName);
                    right.Item().Text(quotation.CustomerAddress);
                    right.Item().Text($"{quotation.CustomerCity}, {quotation.CustomerState}");
                });
            });

            column.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(accentColor).Padding(4).Text("Product").FontColor(Colors.White).Bold();
                    header.Cell().Background(accentColor).Padding(4).AlignRight().Text("Qty").FontColor(Colors.White).Bold();
                    header.Cell().Background(accentColor).Padding(4).AlignRight().Text("Unit Price").FontColor(Colors.White).Bold();
                    header.Cell().Background(accentColor).Padding(4).AlignRight().Text("Disc %").FontColor(Colors.White).Bold();
                    header.Cell().Background(accentColor).Padding(4).AlignRight().Text("GST %").FontColor(Colors.White).Bold();
                    header.Cell().Background(accentColor).Padding(4).AlignRight().Text("Line Total").FontColor(Colors.White).Bold();
                });

                foreach (var line in quotation.Lines)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(line.ProductName);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.Quantity.ToString());
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.UnitPrice.ToString("N2"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.DiscountPercent.ToString("N2"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.GstPercent.ToString("N2"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.TotalAmount.ToString("N2"));
                }
            });

            column.Item().AlignRight().Width(220).Table(totals =>
            {
                totals.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                AddTotalRow(totals, "Subtotal", quotation.SubTotal, accentColor, false);
                AddTotalRow(totals, "Discount", quotation.DiscountAmount, accentColor, false);
                AddTotalRow(totals, "Tax", quotation.TaxAmount, accentColor, false);
                AddTotalRow(totals, "Grand Total", quotation.TotalAmount, accentColor, true);
            });

            if (!string.IsNullOrWhiteSpace(quotation.Notes))
            {
                column.Item().PaddingTop(8).Text(text =>
                {
                    text.Span("Notes: ").Bold();
                    text.Span(quotation.Notes);
                });
            }
        });
    }

    private static void AddTotalRow(TableDescriptor table, string label, decimal amount, Color accentColor, bool emphasize)
    {
        if (emphasize)
        {
            table.Cell().Background(accentColor).Padding(4).Text(label).Bold().FontColor(Colors.White);
            table.Cell().Background(accentColor).Padding(4).AlignRight().Text(amount.ToString("N2")).Bold().FontColor(Colors.White);
            return;
        }

        table.Cell().Padding(4).Text(label);
        table.Cell().Padding(4).AlignRight().Text(amount.ToString("N2"));
    }

    private static void ComposeFooter(IContainer container, CompanySettingsDto company)
    {
        container.AlignCenter().Text(company.InvoiceTerms).FontSize(9).Italic();
    }

    private static Color ParseColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return Color.FromHex("#1F2937");
        }

        var normalized = hex.StartsWith('#') ? hex : $"#{hex}";
        return Color.FromHex(normalized);
    }
}
