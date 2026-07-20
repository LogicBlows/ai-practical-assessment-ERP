namespace SmeErp.Application.Services;

public class QuotationLineCalculationInput
{
    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public decimal DiscountPercent { get; init; }

    public decimal GstPercent { get; init; }
}
