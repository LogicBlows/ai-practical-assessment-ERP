namespace SmeErp.Domain.Entities;

public class Quotation
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string QuotationNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public DateTime QuotationDate { get; set; }

    public DateTime ValidUntil { get; set; }

    public decimal SubTotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public Company Company { get; set; } = null!;

    public Customer Customer { get; set; } = null!;

    public ICollection<QuotationLine> Lines { get; set; } = new List<QuotationLine>();
}
