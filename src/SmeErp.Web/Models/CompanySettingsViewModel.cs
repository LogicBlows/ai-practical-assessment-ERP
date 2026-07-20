using System.ComponentModel.DataAnnotations;
using SmeErp.Application.DTOs;

namespace SmeErp.Web.Models;

public class CompanySettingsViewModel
{
    [Required]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Display(Name = "City")]
    public string City { get; set; } = string.Empty;

    [Required]
    [Display(Name = "State")]
    public string State { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Country")]
    public string Country { get; set; } = string.Empty;

    [Required]
    [Display(Name = "PIN code")]
    public string PinCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "GST number")]
    public string GstNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "PAN number")]
    public string PanNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Mobile")]
    public string Mobile { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Website")]
    public string Website { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Primary color")]
    public string PrimaryColor { get; set; } = "#1F2937";

    [Required]
    [Display(Name = "Invoice terms")]
    public string InvoiceTerms { get; set; } = string.Empty;

    public static CompanySettingsViewModel FromDto(CompanySettingsDto dto) =>
        new()
        {
            CompanyName = dto.CompanyName,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PinCode = dto.PinCode,
            GstNumber = dto.GstNumber,
            PanNumber = dto.PanNumber,
            Mobile = dto.Mobile,
            Email = dto.Email,
            Website = dto.Website,
            PrimaryColor = dto.PrimaryColor,
            InvoiceTerms = dto.InvoiceTerms
        };

    public CompanySettingsDto ToDto() =>
        new()
        {
            CompanyName = CompanyName,
            Address = Address,
            City = City,
            State = State,
            Country = Country,
            PinCode = PinCode,
            GstNumber = GstNumber,
            PanNumber = PanNumber,
            Mobile = Mobile,
            Email = Email,
            Website = Website,
            PrimaryColor = PrimaryColor,
            InvoiceTerms = InvoiceTerms
        };
}
