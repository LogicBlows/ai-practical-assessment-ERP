namespace SmeErp.Application.DTOs;

public class CreateQuotationRequestDto
{
    public int CustomerId { get; set; }

    public DateTime QuotationDate { get; set; }

    public DateTime ValidUntil { get; set; }

    public string? Notes { get; set; }

    public List<QuotationLineInputDto> Lines { get; set; } = new();
}
