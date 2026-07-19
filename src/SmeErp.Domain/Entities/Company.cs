namespace SmeErp.Domain.Entities;

public class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Tagline { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string PinCode { get; set; } = string.Empty;

    public string GstNumber { get; set; } = string.Empty;

    public string PanNumber { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    public string? LogoPath { get; set; }

    public ICollection<CompanySetting> Settings { get; set; } = new List<CompanySetting>();

    public ICollection<Product> Products { get; set; } = new List<Product>();

    public ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
