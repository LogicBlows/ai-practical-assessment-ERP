using SmeErp.Application.Common;
using SmeErp.Application.DTOs;

namespace SmeErp.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<ServiceResult<IReadOnlyList<CustomerListItemDto>>> SearchAsync(
        int companyId,
        string? keyword,
        CancellationToken cancellationToken = default);
}
