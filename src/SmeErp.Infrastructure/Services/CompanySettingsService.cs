using Microsoft.EntityFrameworkCore;
using SmeErp.Application.Common;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Domain.Entities;
using SmeErp.Infrastructure.Persistence;
using SmeErp.Shared.Settings;

namespace SmeErp.Infrastructure.Services;

public class CompanySettingsService : ICompanySettingsService
{
  private const string DefaultPrimaryColor = "#1F2937";
  private const string DefaultInvoiceTerms =
      "Payment is due within the agreed credit period. Goods once sold are not returnable unless agreed in writing.";

  private readonly AppDbContext _dbContext;

  public CompanySettingsService(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<ServiceResult<CompanySettingsDto>> GetAsync(
      int companyId,
      CancellationToken cancellationToken = default)
  {
    if (companyId <= 0)
    {
      return ServiceResult<CompanySettingsDto>.Failure("A valid company is required.");
    }

    var company = await _dbContext.Companies
        .AsNoTracking()
        .Include(c => c.Settings)
        .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);

    if (company is null)
    {
      return ServiceResult<CompanySettingsDto>.Failure("Company not found.");
    }

    var settings = company.Settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

    return ServiceResult<CompanySettingsDto>.Success(MapToDto(company, settings));
  }

  public async Task<ServiceResult> UpdateAsync(
      int companyId,
      CompanySettingsDto dto,
      CancellationToken cancellationToken = default)
  {
    if (companyId <= 0)
    {
      return ServiceResult.Failure("A valid company is required.");
    }

    var company = await _dbContext.Companies
        .Include(c => c.Settings)
        .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);

    if (company is null)
    {
      return ServiceResult.Failure("Company not found.");
    }

    company.Name = dto.CompanyName.Trim();
    company.Address = dto.Address.Trim();
    company.City = dto.City.Trim();
    company.State = dto.State.Trim();
    company.Country = dto.Country.Trim();
    company.PinCode = dto.PinCode.Trim();
    company.GstNumber = dto.GstNumber.Trim();
    company.PanNumber = dto.PanNumber.Trim();
    company.Mobile = dto.Mobile.Trim();
    company.Email = dto.Email.Trim();
    company.Website = dto.Website.Trim();

    UpsertSetting(company, CompanySettingKeys.PrimaryColor, dto.PrimaryColor.Trim());
    UpsertSetting(company, CompanySettingKeys.InvoiceTerms, dto.InvoiceTerms.Trim());

    await _dbContext.SaveChangesAsync(cancellationToken);

    return ServiceResult.Success();
  }

  private static CompanySettingsDto MapToDto(Company company, IReadOnlyDictionary<string, string> settings)
  {
    return new CompanySettingsDto
    {
      CompanyName = company.Name,
      Address = company.Address,
      City = company.City,
      State = company.State,
      Country = company.Country,
      PinCode = company.PinCode,
      GstNumber = company.GstNumber,
      PanNumber = company.PanNumber,
      Mobile = company.Mobile,
      Email = company.Email,
      Website = company.Website,
      PrimaryColor = settings.TryGetValue(CompanySettingKeys.PrimaryColor, out var color) && !string.IsNullOrWhiteSpace(color)
          ? color
          : DefaultPrimaryColor,
      InvoiceTerms = settings.TryGetValue(CompanySettingKeys.InvoiceTerms, out var terms) && !string.IsNullOrWhiteSpace(terms)
          ? terms
          : DefaultInvoiceTerms
    };
  }

  private static void UpsertSetting(Company company, string key, string value)
  {
    var existing = company.Settings.FirstOrDefault(s =>
        string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));

    if (existing is null)
    {
      company.Settings.Add(new CompanySetting
      {
        CompanyId = company.Id,
        Key = key,
        Value = value
      });
    }
    else
    {
      existing.Value = value;
    }
  }
}
