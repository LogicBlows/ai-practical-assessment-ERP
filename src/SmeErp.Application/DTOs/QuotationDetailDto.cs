namespace SmeErp.Application.DTOs;

public class QuotationDetailDto
{
    public int Id { get; set; }

    public string QuotationNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTime QuotationDate { get; set; }

    public DateTime ValidUntil { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public IReadOnlyList<QuotationLineDetailDto> Lines { get; set; } = Array.Empty<QuotationLineDetailDto>();
}
