namespace SmeErp.Application.Services;

public class QuotationLineCalculationResult
{
    public decimal LineSubtotal { get; init; }

    public decimal LineDiscount { get; init; }

    public decimal LineTaxableAmount { get; init; }

    public decimal LineTax { get; init; }

    public decimal LineTotal { get; init; }
}
