namespace SmeErp.Application.DTOs;

public class QuotationLineDetailDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal GstPercent { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }
}
