using Microsoft.EntityFrameworkCore;
using SmeErp.Application.Common;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Persistence;

namespace SmeErp.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<DashboardSummaryDto>> GetSummaryAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return ServiceResult<DashboardSummaryDto>.Failure("A valid company is required.");
        }

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var totalProducts = await _dbContext.Products
            .AsNoTracking()
            .CountAsync(p => p.CompanyId == companyId, cancellationToken);

        var totalCustomers = await _dbContext.Customers
            .AsNoTracking()
            .CountAsync(c => c.CompanyId == companyId, cancellationToken);

        var quotationsToday = await _dbContext.Quotations
            .AsNoTracking()
            .CountAsync(
                q => q.CompanyId == companyId
                    && q.QuotationDate >= today
                    && q.QuotationDate < tomorrow,
                cancellationToken);

        var pendingQuotations = await _dbContext.Quotations
            .AsNoTracking()
            .CountAsync(
                q => q.CompanyId == companyId && q.ValidUntil >= today,
                cancellationToken);

        return ServiceResult<DashboardSummaryDto>.Success(new DashboardSummaryDto
        {
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            TotalQuotationsToday = quotationsToday,
            PendingQuotations = pendingQuotations
        });
    }
}
