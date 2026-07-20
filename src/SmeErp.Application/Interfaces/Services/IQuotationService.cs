using SmeErp.Application.Common;
using SmeErp.Application.DTOs;

namespace SmeErp.Application.Interfaces.Services;

public interface IQuotationService
{
    Task<ServiceResult<int>> CreateAsync(
        int companyId,
        CreateQuotationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<QuotationListItemDto>>> GetListAsync(
        int companyId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<QuotationDetailDto>> GetDetailAsync(
        int companyId,
        int quotationId,
        CancellationToken cancellationToken = default);
}
