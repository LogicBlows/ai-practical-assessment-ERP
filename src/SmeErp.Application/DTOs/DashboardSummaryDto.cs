namespace SmeErp.Application.DTOs;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }

    public int TotalCustomers { get; set; }

    public int TotalQuotationsToday { get; set; }

    public int PendingQuotations { get; set; }
}
