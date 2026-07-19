namespace SmeErp.Domain.Entities;

public class QuotationLine
{
    public int Id { get; set; }

    public int QuotationId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal GstPercent { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public Quotation Quotation { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
