using Microsoft.AspNetCore.Mvc;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Services;
using SmeErp.Web.Models;

namespace SmeErp.Web.ViewComponents;

public class CompanyBrandingViewComponent : ViewComponent
{
    private const string DefaultPrimaryColor = "#2C3E50";

    private readonly ICurrentCompanyService _currentCompanyService;
    private readonly ICompanySettingsService _companySettingsService;

    public CompanyBrandingViewComponent(
        ICurrentCompanyService currentCompanyService,
        ICompanySettingsService companySettingsService)
    {
        _currentCompanyService = currentCompanyService;
        _companySettingsService = companySettingsService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = await BuildBrandingAsync();
        HttpContext.Items["BrandTitle"] = model.BrandTitle;
        return View(model);
    }

    private async Task<CompanyBrandingViewModel> BuildBrandingAsync()
    {
        if (HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return new CompanyBrandingViewModel();
        }

        var companyId = await _currentCompanyService.GetCompanyIdAsync();
        if (companyId is null)
        {
            return new CompanyBrandingViewModel();
        }

        var result = await _companySettingsService.GetAsync(companyId.Value);
        if (!result.Succeeded || result.Data is null)
        {
            return new CompanyBrandingViewModel();
        }

        var primaryColor = string.IsNullOrWhiteSpace(result.Data.PrimaryColor)
            ? DefaultPrimaryColor
            : result.Data.PrimaryColor.StartsWith('#')
                ? result.Data.PrimaryColor
                : $"#{result.Data.PrimaryColor}";

        return new CompanyBrandingViewModel
        {
            PrimaryColor = primaryColor,
            CompanyName = result.Data.CompanyName
        };
    }
}
