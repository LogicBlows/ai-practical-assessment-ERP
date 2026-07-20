using SmeErp.Application.DTOs;

namespace SmeErp.Web.Models;

public class DashboardIndexViewModel
{
    public DashboardSummaryDto Summary { get; set; } = new();
}
