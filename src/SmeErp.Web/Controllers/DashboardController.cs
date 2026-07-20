using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmeErp.Infrastructure.Services;

namespace SmeErp.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ICurrentCompanyService _currentCompanyService;

    public DashboardController(ICurrentCompanyService currentCompanyService)
    {
        _currentCompanyService = currentCompanyService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["CompanyId"] = await _currentCompanyService.GetCompanyIdAsync();
        return View();
    }
}
