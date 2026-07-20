using System.ComponentModel.DataAnnotations;
using SmeErp.Application.DTOs;

namespace SmeErp.Web.Models;

public class QuotationIndexViewModel
{
    public IReadOnlyList<QuotationListItemDto> Quotations { get; set; } = Array.Empty<QuotationListItemDto>();
}
