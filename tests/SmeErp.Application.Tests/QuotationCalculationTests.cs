using SmeErp.Application.Services;

namespace SmeErp.Application.Tests;

public class QuotationCalculationTests
{
    [Fact]
    public void Calculate_WithKnownLineItem_ProducesExpectedTotals()
    {
        var totals = QuotationTotalsCalculator.Calculate(new[]
        {
            new QuotationLineCalculationInput
            {
                Quantity = 3,
                UnitPrice = 62.00m,
                DiscountPercent = 6m,
                GstPercent = 18m
            }
        });

        Assert.Equal(186.00m, totals.SubTotal);
        Assert.Equal(11.16m, totals.DiscountAmount);
        Assert.Equal(31.47m, decimal.Round(totals.TaxAmount, 2, MidpointRounding.AwayFromZero));
        Assert.Equal(206.31m, decimal.Round(totals.TotalAmount, 2, MidpointRounding.AwayFromZero));
    }
}
