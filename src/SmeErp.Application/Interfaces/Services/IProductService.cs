using SmeErp.Application.Common;
using SmeErp.Application.DTOs;

namespace SmeErp.Application.Interfaces.Services;

public interface IProductService
{
    Task<ServiceResult<IReadOnlyList<ProductListItemDto>>> SearchAsync(
        int companyId,
        string? keyword,
        CancellationToken cancellationToken = default);
}
