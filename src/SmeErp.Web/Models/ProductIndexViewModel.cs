using SmeErp.Application.DTOs;

namespace SmeErp.Web.Models;

public class ProductIndexViewModel
{
    public string? Search { get; set; }

    public IReadOnlyList<ProductListItemDto> Products { get; set; } = Array.Empty<ProductListItemDto>();
}
