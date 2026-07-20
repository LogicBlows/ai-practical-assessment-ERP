using Microsoft.EntityFrameworkCore;
using SmeErp.Application.Common;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Persistence;

namespace SmeErp.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _dbContext;

    public CustomerService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<IReadOnlyList<CustomerListItemDto>>> SearchAsync(
        int companyId,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return ServiceResult<IReadOnlyList<CustomerListItemDto>>.Failure("A valid company is required.");
        }

        var query = _dbContext.Customers
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(c =>
                c.Name.Contains(term) ||
                c.Code.Contains(term));
        }

        var customers = await query
            .OrderBy(c => c.Name)
            .Select(c => new CustomerListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Mobile = c.Mobile,
                City = c.City,
                State = c.State,
                Address = c.Address
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<CustomerListItemDto>>.Success(customers);
    }
}
