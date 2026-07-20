using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Services;
using SmeErp.Web.Models;

namespace SmeErp.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ICurrentCompanyService _currentCompanyService;
    private readonly ICompanySettingsService _companySettingsService;

    public SettingsController(
        ICurrentCompanyService currentCompanyService,
        ICompanySettingsService companySettingsService)
    {
        _currentCompanyService = currentCompanyService;
        _companySettingsService = companySettingsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var companyId = await _currentCompanyService.GetCompanyIdAsync();
        if (companyId is null)
        {
            return Challenge();
        }

        var result = await _companySettingsService.GetAsync(companyId.Value);
        if (!result.Succeeded || result.Data is null)
        {
            return View("Error");
        }

        return View(CompanySettingsViewModel.FromDto(result.Data));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CompanySettingsViewModel model)
    {
        var companyId = await _currentCompanyService.GetCompanyIdAsync();
        if (companyId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _companySettingsService.UpdateAsync(companyId.Value, model.ToDto());
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Unable to save settings.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Company settings saved successfully.";
        return RedirectToAction(nameof(Index));
    }
}
