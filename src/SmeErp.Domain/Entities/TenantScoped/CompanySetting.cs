namespace SmeErp.Domain.Entities;

public class CompanySetting
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public Company Company { get; set; } = null!;
}
