namespace SmeErp.Web.Models;

public class CompanyBrandingViewModel
{
    public string PrimaryColor { get; init; } = "#2C3E50";

    public string? CompanyName { get; init; }

    public string BrandTitle => string.IsNullOrWhiteSpace(CompanyName) ? "SmeErp" : CompanyName;
}
