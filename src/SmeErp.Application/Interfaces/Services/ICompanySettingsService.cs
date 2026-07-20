using SmeErp.Application.Common;
using SmeErp.Application.DTOs;

namespace SmeErp.Application.Interfaces.Services;

public interface ICompanySettingsService
{
    Task<ServiceResult<CompanySettingsDto>> GetAsync(
        int companyId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(
        int companyId,
        CompanySettingsDto dto,
        CancellationToken cancellationToken = default);
}
