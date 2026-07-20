using SmeErp.Application.Common;
using SmeErp.Application.DTOs;

namespace SmeErp.Application.Interfaces.Services;

public interface ISearchService
{
    Task<ServiceResult<GlobalSearchResultDto>> SearchAsync(
        int companyId,
        string? keyword,
        CancellationToken cancellationToken = default);
}
