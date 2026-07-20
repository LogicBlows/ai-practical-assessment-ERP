using SmeErp.Application.Common;
using SmeErp.Application.DTOs;

namespace SmeErp.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<ServiceResult<DashboardSummaryDto>> GetSummaryAsync(
        int companyId,
        CancellationToken cancellationToken = default);
}
