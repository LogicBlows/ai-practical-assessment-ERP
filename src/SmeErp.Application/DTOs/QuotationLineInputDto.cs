namespace SmeErp.Application.DTOs;

public class QuotationLineInputDto
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountPercent { get; set; }
}
