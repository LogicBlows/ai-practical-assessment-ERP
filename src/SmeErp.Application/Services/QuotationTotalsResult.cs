namespace SmeErp.Application.Services;

public class QuotationTotalsResult
{
    public decimal SubTotal { get; init; }

    public decimal DiscountAmount { get; init; }

    public decimal TaxAmount { get; init; }

    public decimal TotalAmount { get; init; }

    public IReadOnlyList<QuotationLineCalculationResult> Lines { get; init; } =
        Array.Empty<QuotationLineCalculationResult>();
}
