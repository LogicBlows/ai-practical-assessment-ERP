using System.ComponentModel.DataAnnotations;
using SmeErp.Application.DTOs;

namespace SmeErp.Web.Models;

public class CreateQuotationViewModel
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a customer.")]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Quotation date")]
    public DateTime QuotationDate { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Valid until")]
    public DateTime ValidUntil { get; set; } = DateTime.Today.AddDays(30);

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    public List<QuotationLineInputDto> Lines { get; set; } = new()
    {
        new QuotationLineInputDto()
    };

    public IReadOnlyList<CustomerListItemDto> Customers { get; set; } = Array.Empty<CustomerListItemDto>();

    public IReadOnlyList<ProductListItemDto> Products { get; set; } = Array.Empty<ProductListItemDto>();
}
