using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Services;
using SmeErp.Web.Models;

namespace SmeErp.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ICurrentCompanyService _currentCompanyService;
    private readonly ICustomerService _customerService;

    public CustomersController(
        ICurrentCompanyService currentCompanyService,
        ICustomerService customerService)
    {
        _currentCompanyService = currentCompanyService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var companyId = await _currentCompanyService.GetCompanyIdAsync();
        if (companyId is null)
        {
            return Challenge();
        }

        var result = await _customerService.SearchAsync(companyId.Value, search);
        if (!result.Succeeded)
        {
            return View("Error");
        }

        var viewModel = new CustomerIndexViewModel
        {
            Search = search,
            Customers = result.Data ?? Array.Empty<CustomerListItemDto>()
        };

        return View(viewModel);
    }
}
