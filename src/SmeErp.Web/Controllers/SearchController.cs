using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Services;
using SmeErp.Web.Models;

namespace SmeErp.Web.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly ICurrentCompanyService _currentCompanyService;
    private readonly ISearchService _searchService;

    public SearchController(
        ICurrentCompanyService currentCompanyService,
        ISearchService searchService)
    {
        _currentCompanyService = currentCompanyService;
        _searchService = searchService;
    }

    public async Task<IActionResult> Index(string? keyword)
    {
        var companyId = await _currentCompanyService.GetCompanyIdAsync();
        if (companyId is null)
        {
            return Challenge();
        }

        var result = await _searchService.SearchAsync(companyId.Value, keyword);
        if (!result.Succeeded || result.Data is null)
        {
            return View("Error");
        }

        var viewModel = new SearchIndexViewModel
        {
            Keyword = keyword,
            Results = result.Data
        };

        return View(viewModel);
    }
}
