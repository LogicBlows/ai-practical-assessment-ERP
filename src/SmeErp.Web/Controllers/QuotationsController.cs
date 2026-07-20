using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmeErp.Application.DTOs;
using SmeErp.Application.Interfaces.Services;
using SmeErp.Infrastructure.Services;
using SmeErp.Web.Models;

namespace SmeErp.Web.Controllers;

[Authorize]
public class QuotationsController : Controller
{
    private readonly ICurrentCompanyService _currentCompanyService;
    private readonly IQuotationService _quotationService;
    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;

    public QuotationsController(
        ICurrentCompanyService currentCompanyService,
        IQuotationService quotationService,
        ICustomerService customerService,
        IProductService productService)
    {
        _currentCompanyService = currentCompanyService;
        _quotationService = quotationService;
        _customerService = customerService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var companyId = await GetCompanyIdOrChallengeAsync();
        if (companyId is null)
        {
            return Challenge();
        }

        var result = await _quotationService.GetListAsync(companyId.Value);
        if (!result.Succeeded)
        {
            return View("Error");
        }

        return View(new QuotationIndexViewModel
        {
            Quotations = result.Data ?? Array.Empty<QuotationListItemDto>()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var companyId = await GetCompanyIdOrChallengeAsync();
        if (companyId is null)
        {
            return Challenge();
        }

        var model = await BuildCreateViewModelAsync(companyId.Value, new CreateQuotationViewModel());
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuotationViewModel model)
    {
        var companyId = await GetCompanyIdOrChallengeAsync();
        if (companyId is null)
        {
            return Challenge();
        }

        model.Lines = model.Lines?
            .Where(l => l.ProductId > 0)
            .ToList() ?? new List<QuotationLineInputDto>();

        if (model.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "At least one line item is required.");
        }

        if (!ModelState.IsValid)
        {
            model = await BuildCreateViewModelAsync(companyId.Value, model);
            return View(model);
        }

        var request = new CreateQuotationRequestDto
        {
            CustomerId = model.CustomerId,
            QuotationDate = model.QuotationDate,
            ValidUntil = model.ValidUntil,
            Notes = model.Notes,
            Lines = model.Lines
        };

        var result = await _quotationService.CreateAsync(companyId.Value, request);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Unable to create quotation.");
            model = await BuildCreateViewModelAsync(companyId.Value, model);
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data });
    }

    public async Task<IActionResult> Details(int id)
    {
        var companyId = await GetCompanyIdOrChallengeAsync();
        if (companyId is null)
        {
            return Challenge();
        }

        var result = await _quotationService.GetDetailAsync(companyId.Value, id);
        if (!result.Succeeded)
        {
            return NotFound();
        }

        return View(result.Data);
    }

    private async Task<int?> GetCompanyIdOrChallengeAsync()
    {
        return await _currentCompanyService.GetCompanyIdAsync();
    }

    private async Task<CreateQuotationViewModel> BuildCreateViewModelAsync(
        int companyId,
        CreateQuotationViewModel model)
    {
        var customersResult = await _customerService.SearchAsync(companyId, null);
        var productsResult = await _productService.SearchAsync(companyId, null);

        model.Customers = customersResult.Data ?? Array.Empty<CustomerListItemDto>();
        model.Products = productsResult.Data ?? Array.Empty<ProductListItemDto>();

        if (model.Lines.Count == 0)
        {
            model.Lines.Add(new QuotationLineInputDto());
        }

        return model;
    }
}
