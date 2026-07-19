namespace SmeErp.Domain.Entities;

public class Customer
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public Company Company { get; set; } = null!;

    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
