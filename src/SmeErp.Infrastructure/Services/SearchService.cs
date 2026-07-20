using Microsoft.EntityFrameworkCore;
using SmeErp.Application.Common;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Persistence;

namespace SmeErp.Infrastructure.Services;

public class SearchService : ISearchService
{
    private const int MinimumKeywordLength = 2;

    private readonly AppDbContext _dbContext;

    public SearchService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<GlobalSearchResultDto>> SearchAsync(
        int companyId,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return ServiceResult<GlobalSearchResultDto>.Failure("A valid company is required.");
        }

        var trimmedKeyword = keyword?.Trim() ?? string.Empty;
        if (trimmedKeyword.Length < MinimumKeywordLength)
        {
            return ServiceResult<GlobalSearchResultDto>.Success(new GlobalSearchResultDto
            {
                Keyword = trimmedKeyword,
                KeywordTooShort = !string.IsNullOrEmpty(trimmedKeyword)
            });
        }

        var products = await SearchProductsAsync(companyId, trimmedKeyword, cancellationToken);
        var customers = await SearchCustomersAsync(companyId, trimmedKeyword, cancellationToken);
        var quotations = await SearchQuotationsAsync(companyId, trimmedKeyword, cancellationToken);

        return ServiceResult<GlobalSearchResultDto>.Success(new GlobalSearchResultDto
        {
            Keyword = trimmedKeyword,
            Products = products,
            Customers = customers,
            Quotations = quotations
        });
    }

    private async Task<IReadOnlyList<SearchResultItemDto>> SearchProductsAsync(
        int companyId,
        string term,
        CancellationToken cancellationToken)
    {
        var matches = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .Where(p =>
                p.Name.Contains(term) ||
                p.Sku.Contains(term) ||
                p.Barcode.Contains(term))
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Sku })
            .ToListAsync(cancellationToken);

        return matches
            .Select(p => new SearchResultItemDto
            {
                Id = p.Id,
                Type = SearchResultType.Product,
                DisplayName = p.Name,
                Subtitle = p.Sku,
                Url = $"/Products?search={Uri.EscapeDataString(p.Name)}"
            })
            .ToList();
    }

    private async Task<IReadOnlyList<SearchResultItemDto>> SearchCustomersAsync(
        int companyId,
        string term,
        CancellationToken cancellationToken)
    {
        var matches = await _dbContext.Customers
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .Where(c =>
                c.Name.Contains(term) ||
                c.Code.Contains(term))
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Code })
            .ToListAsync(cancellationToken);

        return matches
            .Select(c => new SearchResultItemDto
            {
                Id = c.Id,
                Type = SearchResultType.Customer,
                DisplayName = c.Name,
                Subtitle = c.Code,
                Url = $"/Customers?search={Uri.EscapeDataString(c.Name)}"
            })
            .ToList();
    }

    private async Task<IReadOnlyList<SearchResultItemDto>> SearchQuotationsAsync(
        int companyId,
        string term,
        CancellationToken cancellationToken)
    {
        var matches = await _dbContext.Quotations
            .AsNoTracking()
            .Where(q => q.CompanyId == companyId)
            .Where(q =>
                q.QuotationNumber.Contains(term) ||
                q.Customer.Name.Contains(term))
            .OrderByDescending(q => q.QuotationDate)
            .Select(q => new { q.Id, q.QuotationNumber, CustomerName = q.Customer.Name })
            .ToListAsync(cancellationToken);

        return matches
            .Select(q => new SearchResultItemDto
            {
                Id = q.Id,
                Type = SearchResultType.Quotation,
                DisplayName = q.QuotationNumber,
                Subtitle = q.CustomerName,
                Url = $"/Quotations/Details/{q.Id}"
            })
            .ToList();
    }
}
