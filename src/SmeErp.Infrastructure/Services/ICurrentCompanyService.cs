namespace SmeErp.Infrastructure.Services;

public interface ICurrentCompanyService
{
    Task<int?> GetCompanyIdAsync(CancellationToken cancellationToken = default);
}
