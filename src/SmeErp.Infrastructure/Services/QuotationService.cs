using Microsoft.EntityFrameworkCore;
using SmeErp.Application.Common;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Domain.Entities;
using SmeErp.Infrastructure.Persistence;

namespace SmeErp.Infrastructure.Services;

public class QuotationService : IQuotationService
{
    private readonly AppDbContext _dbContext;

    public QuotationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<int>> CreateAsync(
        int companyId,
        CreateQuotationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return ServiceResult<int>.Failure("A valid company is required.");
        }

        var customerExists = await _dbContext.Customers
            .AnyAsync(c => c.Id == request.CustomerId && c.CompanyId == companyId, cancellationToken);

        if (!customerExists)
        {
            return ServiceResult<int>.Failure("Customer not found or does not belong to your company.");
        }

        if (request.Lines is null || request.Lines.Count == 0)
        {
            return ServiceResult<int>.Failure("At least one line item is required.");
        }

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(p => p.CompanyId == companyId && productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
            {
                return ServiceResult<int>.Failure("Each line must have a quantity greater than zero.");
            }

            if (!products.ContainsKey(line.ProductId))
            {
                return ServiceResult<int>.Failure("One or more products are invalid for your company.");
            }
        }

        var quotationLines = new List<QuotationLine>();
        decimal subTotal = 0m;
        decimal discountAmount = 0m;
        decimal taxAmount = 0m;
        decimal totalAmount = 0m;

        foreach (var input in request.Lines)
        {
            var product = products[input.ProductId];
            var lineSubtotal = input.Quantity * input.UnitPrice;
            var lineDiscount = lineSubtotal * (input.DiscountPercent / 100m);
            var lineTaxableAmount = lineSubtotal - lineDiscount;
            var lineTax = lineTaxableAmount * (product.GstPercent / 100m);
            var lineTotal = lineTaxableAmount + lineTax;

            subTotal += lineSubtotal;
            discountAmount += lineDiscount;
            taxAmount += lineTax;
            totalAmount += lineTotal;

            quotationLines.Add(new QuotationLine
            {
                ProductId = input.ProductId,
                Quantity = input.Quantity,
                UnitPrice = input.UnitPrice,
                DiscountPercent = input.DiscountPercent,
                GstPercent = product.GstPercent,
                TaxAmount = lineTax,
                TotalAmount = lineTotal
            });
        }

        var quotation = new Quotation
        {
            CompanyId = companyId,
            QuotationNumber = await GenerateQuotationNumberAsync(companyId, cancellationToken),
            CustomerId = request.CustomerId,
            QuotationDate = request.QuotationDate.Date,
            ValidUntil = request.ValidUntil.Date,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            Notes = request.Notes,
            Lines = quotationLines
        };

        _dbContext.Quotations.Add(quotation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<int>.Success(quotation.Id);
    }

    public async Task<ServiceResult<IReadOnlyList<QuotationListItemDto>>> GetListAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return ServiceResult<IReadOnlyList<QuotationListItemDto>>.Failure("A valid company is required.");
        }

        var quotations = await _dbContext.Quotations
            .AsNoTracking()
            .Where(q => q.CompanyId == companyId)
            .OrderByDescending(q => q.QuotationDate)
            .ThenByDescending(q => q.Id)
            .Select(q => new QuotationListItemDto
            {
                Id = q.Id,
                QuotationNumber = q.QuotationNumber,
                CustomerName = q.Customer.Name,
                QuotationDate = q.QuotationDate,
                TotalAmount = q.TotalAmount
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<QuotationListItemDto>>.Success(quotations);
    }

    public async Task<ServiceResult<QuotationDetailDto>> GetDetailAsync(
        int companyId,
        int quotationId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return ServiceResult<QuotationDetailDto>.Failure("A valid company is required.");
        }

        var quotation = await _dbContext.Quotations
            .AsNoTracking()
            .Where(q => q.Id == quotationId && q.CompanyId == companyId)
            .Select(q => new QuotationDetailDto
            {
                Id = q.Id,
                QuotationNumber = q.QuotationNumber,
                CustomerId = q.CustomerId,
                CustomerName = q.Customer.Name,
                CustomerAddress = q.Customer.Address,
                CustomerCity = q.Customer.City,
                CustomerState = q.Customer.State,
                QuotationDate = q.QuotationDate,
                ValidUntil = q.ValidUntil,
                SubTotal = q.SubTotal,
                DiscountAmount = q.DiscountAmount,
                TaxAmount = q.TaxAmount,
                TotalAmount = q.TotalAmount,
                Notes = q.Notes,
                Lines = q.Lines
                    .OrderBy(l => l.Id)
                    .Select(l => new QuotationLineDetailDto
                    {
                        ProductId = l.ProductId,
                        ProductName = l.Product.Name,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountPercent = l.DiscountPercent,
                        GstPercent = l.GstPercent,
                        TaxAmount = l.TaxAmount,
                        TotalAmount = l.TotalAmount
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (quotation is null)
        {
            return ServiceResult<QuotationDetailDto>.Failure("Quotation not found.");
        }

        return ServiceResult<QuotationDetailDto>.Success(quotation);
    }

    private async Task<string> GenerateQuotationNumberAsync(int companyId, CancellationToken cancellationToken)
    {
        var count = await _dbContext.Quotations
            .CountAsync(q => q.CompanyId == companyId, cancellationToken);

        return $"QT-{companyId}-{(count + 1):D5}";
    }
}
