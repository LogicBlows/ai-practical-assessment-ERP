using Microsoft.EntityFrameworkCore;
using SmeErp.Application.Common;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Persistence;

namespace SmeErp.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;

    public ProductService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<IReadOnlyList<ProductListItemDto>>> SearchAsync(
        int companyId,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return ServiceResult<IReadOnlyList<ProductListItemDto>>.Failure("A valid company is required.");
        }

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                p.Sku.Contains(term) ||
                p.Barcode.Contains(term));
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Barcode = p.Barcode,
                SellingPrice = p.SellingPrice,
                GstPercent = p.GstPercent,
                CurrentStock = p.CurrentStock
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<ProductListItemDto>>.Success(products);
    }
}
