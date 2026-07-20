namespace SmeErp.Application.DTOs;

public class QuotationListItemDto
{
    public int Id { get; set; }

    public string QuotationNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public DateTime QuotationDate { get; set; }

    public decimal TotalAmount { get; set; }
}
