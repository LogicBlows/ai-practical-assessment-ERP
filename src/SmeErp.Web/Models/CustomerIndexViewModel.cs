using SmeErp.Application.DTOs;

namespace SmeErp.Web.Models;

public class CustomerIndexViewModel
{
    public string? Search { get; set; }

    public IReadOnlyList<CustomerListItemDto> Customers { get; set; } = Array.Empty<CustomerListItemDto>();
}
