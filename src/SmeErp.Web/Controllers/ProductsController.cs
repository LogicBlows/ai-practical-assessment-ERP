using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Services;
using SmeErp.Web.Models;

namespace SmeErp.Web.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly ICurrentCompanyService _currentCompanyService;
    private readonly IProductService _productService;

    public ProductsController(
        ICurrentCompanyService currentCompanyService,
        IProductService productService)
    {
        _currentCompanyService = currentCompanyService;
        _productService = productService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var companyId = await _currentCompanyService.GetCompanyIdAsync();
        if (companyId is null)
        {
            return Challenge();
        }

        var result = await _productService.SearchAsync(companyId.Value, search);
        if (!result.Succeeded)
        {
            return View("Error");
        }

        var viewModel = new ProductIndexViewModel
        {
            Search = search,
            Products = result.Data ?? Array.Empty<ProductListItemDto>()
        };

        return View(viewModel);
    }
}
