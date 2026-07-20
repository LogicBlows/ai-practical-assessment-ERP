namespace SmeErp.Application.Services;

public static class QuotationTotalsCalculator
{
    public static QuotationTotalsResult Calculate(IEnumerable<QuotationLineCalculationInput> lines)
    {
        var lineResults = lines.Select(CalculateLine).ToList();

        return new QuotationTotalsResult
        {
            SubTotal = lineResults.Sum(l => l.LineSubtotal),
            DiscountAmount = lineResults.Sum(l => l.LineDiscount),
            TaxAmount = lineResults.Sum(l => l.LineTax),
            TotalAmount = lineResults.Sum(l => l.LineTotal),
            Lines = lineResults
        };
    }

    public static QuotationLineCalculationResult CalculateLine(QuotationLineCalculationInput line)
    {
        var lineSubtotal = line.Quantity * line.UnitPrice;
        var lineDiscount = lineSubtotal * (line.DiscountPercent / 100m);
        var lineTaxableAmount = lineSubtotal - lineDiscount;
        var lineTax = lineTaxableAmount * (line.GstPercent / 100m);
        var lineTotal = lineTaxableAmount + lineTax;

        return new QuotationLineCalculationResult
        {
            LineSubtotal = lineSubtotal,
            LineDiscount = lineDiscount,
            LineTaxableAmount = lineTaxableAmount,
            LineTax = lineTax,
            LineTotal = lineTotal
        };
    }
}
