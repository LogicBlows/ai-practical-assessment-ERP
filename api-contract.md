# SmeErp API Contract (MVC Actions)

## Overview

SmeErp is an **ASP.NET Core MVC** application, not a separate REST API. The "endpoints" documented here are **MVC controller actions** that render Razor views, perform redirects, or return file downloads. There are **no JSON/AJAX API endpoints** in the current codebase.

Default routing (from `Program.cs`):

```
{controller=Home}/{action=Index}/{id?}
```

Unless noted, actions accept both GET and POST only where explicitly marked. Unauthenticated access to `[Authorize]` controllers redirects to `/Account/Login` (configured in `Program.cs`).

**Role restrictions:** No controller action uses `[Authorize(Roles = "...")]`. Authentication is required where `[Authorize]` is applied; any authenticated user with a valid `CompanyId` can access protected actions. Seeded users have the `Proprietor` role, but the controllers do not enforce role checks.

**Tenant scoping:** Protected business actions resolve the current user's `CompanyId` via `ICurrentCompanyService` and pass it to Application-layer services. If `CompanyId` is null, the controller returns `Challenge()` (redirect to login).

---

## AccountController

Uses ASP.NET Identity `SignInManager<ApplicationUser>` directly (no Application-layer service).

### `GET /Account/Login`

| | |
|---|---|
| **Purpose** | Display the sign-in form. |
| **Authorization** | `[AllowAnonymous]` |
| **Inputs** | Query: `returnUrl` (optional) — local URL to redirect to after successful login. |
| **Response** | If already authenticated → redirect to `/Dashboard/Index`. Otherwise → renders `Views/Account/Login.cshtml` with an empty `LoginViewModel`. |
| **Validation** | None (display only). |

### `POST /Account/Login`

| | |
|---|---|
| **Purpose** | Authenticate a user with email and password. |
| **Authorization** | `[AllowAnonymous]` |
| **Inputs** | Form fields: `Email`, `Password`, `RememberMe`; hidden `returnUrl` (optional). Anti-forgery token required (`[ValidateAntiForgeryToken]`). |
| **Response** | On success → redirect to `returnUrl` if it is a local URL, otherwise `/Dashboard/Index`. On failure → re-render login view with model errors. |
| **Validation** | **Model binding / data annotations:** `Email` — required, valid email format; `Password` — required. **Sign-in:** `SignInManager.PasswordSignInAsync` with `lockoutOnFailure: false`; invalid credentials add model error `"Invalid email or password."` |

### `POST /Account/Logout`

| | |
|---|---|
| **Purpose** | Sign out the current user. |
| **Authorization** | No `[Authorize]` attribute (callable when signed in; typically submitted from the navbar form). Anti-forgery token required. |
| **Inputs** | None (form POST with anti-forgery token only). |
| **Response** | Redirect to `/Account/Login`. |
| **Validation** | None. |

---

## DashboardController

**Class-level:** `[Authorize]`  
**Services:** `ICurrentCompanyService`, `IDashboardService`

### `GET /Dashboard` (or `/Dashboard/Index`)

| | |
|---|---|
| **Purpose** | Display company-scoped KPI summary cards. |
| **Authorization** | `[Authorize]` (any authenticated user) |
| **Inputs** | None. |
| **Response** | On success → renders `Views/Dashboard/Index.cshtml` with `DashboardIndexViewModel` containing `DashboardSummaryDto` (`TotalProducts`, `TotalCustomers`, `TotalQuotationsToday`, `PendingQuotations`). If `CompanyId` is null → `Challenge()`. If service fails → `Views/Shared/Error.cshtml`. |
| **Validation** | **Service (`IDashboardService.GetSummaryAsync`):** `companyId` must be > 0; counts are scoped to the user's company. `TotalQuotationsToday` = quotations with `QuotationDate` on today; `PendingQuotations` = quotations with `ValidUntil >= today`. |

---

## ProductsController

**Class-level:** `[Authorize]`  
**Services:** `ICurrentCompanyService`, `IProductService`

### `GET /Products` (or `/Products/Index`)

| | |
|---|---|
| **Purpose** | List products for the current company with optional keyword search. |
| **Authorization** | `[Authorize]` |
| **Inputs** | Query: `search` (optional) — filters by product name, SKU, or barcode. |
| **Response** | Renders `Views/Products/Index.cshtml` with `ProductIndexViewModel` (`Search`, `Products`). If `CompanyId` is null → `Challenge()`. If service fails → error view. |
| **Validation** | **Service (`IProductService.SearchAsync`):** `companyId` must be > 0. Keyword is trimmed; when provided, matches `Name`, `Sku`, or `Barcode` (contains). Empty keyword returns all company products ordered by name. |

---

## CustomersController

**Class-level:** `[Authorize]`  
**Services:** `ICurrentCompanyService`, `ICustomerService`

### `GET /Customers` (or `/Customers/Index`)

| | |
|---|---|
| **Purpose** | List customers for the current company with optional keyword search. |
| **Authorization** | `[Authorize]` |
| **Inputs** | Query: `search` (optional) — filters by customer name or code. |
| **Response** | Renders `Views/Customers/Index.cshtml` with `CustomerIndexViewModel` (`Search`, `Customers`). If `CompanyId` is null → `Challenge()`. If service fails → error view. |
| **Validation** | **Service (`ICustomerService.SearchAsync`):** `companyId` must be > 0. Keyword is trimmed; when provided, matches `Name` or `Code` (contains). Empty keyword returns all company customers ordered by name. |

---

## QuotationsController

**Class-level:** `[Authorize]`  
**Services:** `ICurrentCompanyService`, `IQuotationService`, `ICustomerService`, `IProductService`, `ICompanySettingsService`, `IQuotationPdfService`

### `GET /Quotations` (or `/Quotations/Index`)

| | |
|---|---|
| **Purpose** | List quotations for the current company, newest first. |
| **Authorization** | `[Authorize]` |
| **Inputs** | None. |
| **Response** | Renders `Views/Quotations/Index.cshtml` with `QuotationIndexViewModel`. If `CompanyId` is null → `Challenge()`. If service fails → error view. |
| **Validation** | **Service (`IQuotationService.GetListAsync`):** `companyId` must be > 0; returns quotations scoped to company, ordered by `QuotationDate` descending then `Id` descending. |

### `GET /Quotations/Create`

| | |
|---|---|
| **Purpose** | Display the quotation creation form with customer and product dropdowns. |
| **Authorization** | `[Authorize]` |
| **Inputs** | None. |
| **Response** | Renders `Views/Quotations/Create.cshtml` with `CreateQuotationViewModel` populated via `ICustomerService.SearchAsync` and `IProductService.SearchAsync` (all customers/products for the company). Includes one empty line item row if none exist. |
| **Validation** | None (display only). |

### `POST /Quotations/Create`

| | |
|---|---|
| **Purpose** | Create a new quotation with line items. |
| **Authorization** | `[Authorize]` |
| **Inputs** | Form fields: `CustomerId`, `QuotationDate`, `ValidUntil`, `Notes` (optional); repeating `Lines[i].ProductId`, `Lines[i].Quantity`, `Lines[i].UnitPrice`, `Lines[i].DiscountPercent`. Anti-forgery token required. |
| **Response** | On success → redirect to `/Quotations/Details/{id}`. On validation or service failure → re-render create view with errors and repopulated dropdowns. |
| **Validation** | **Controller:** Lines with `ProductId <= 0` are stripped before validation; at least one line with `ProductId > 0` is required (`"At least one line item is required."`). **Model binding / data annotations:** `CustomerId` — required, range ≥ 1; `QuotationDate` — required; `ValidUntil` — required. **Service (`IQuotationService.CreateAsync`):** `companyId` > 0; `CustomerId` must belong to the company; at least one line; each line `Quantity` > 0; each `ProductId` must belong to the company; GST percent is taken from the product (not user input); dates stored as `.Date`; quotation number auto-generated as `QT-{companyId}-{sequential}`. |

### `GET /Quotations/Details/{id}`

| | |
|---|---|
| **Purpose** | Display a read-only quotation detail view. |
| **Authorization** | `[Authorize]` |
| **Inputs** | Route: `id` (int) — quotation ID. |
| **Response** | Renders `Views/Quotations/Details.cshtml` with `QuotationDetailDto`. If quotation not found or not in user's company → `404 NotFound`. If `CompanyId` is null → `Challenge()`. |
| **Validation** | **Service (`IQuotationService.GetDetailAsync`):** `companyId` > 0; quotation must match both `id` and `companyId`. |

### `GET /Quotations/DownloadPdf/{id}`

| | |
|---|---|
| **Purpose** | Download a branded PDF of a quotation. |
| **Authorization** | `[Authorize]` |
| **Inputs** | Route: `id` (int) — quotation ID. |
| **Response** | **File download** (`application/pdf`, filename `{QuotationNumber}.pdf`) — this is the only non-HTML response in the application. If quotation not found → `404 NotFound`. If settings load fails → error view. If `CompanyId` is null → `Challenge()`. |
| **Validation** | Loads quotation via `IQuotationService.GetDetailAsync` (company-scoped) and settings via `ICompanySettingsService.GetAsync`; PDF generated by `IQuotationPdfService.GeneratePdf` (no additional validation beyond successful load). |

---

## SettingsController

**Class-level:** `[Authorize]`  
**Services:** `ICurrentCompanyService`, `ICompanySettingsService`

### `GET /Settings` (or `/Settings/Index`)

| | |
|---|---|
| **Purpose** | Display the current company's profile and settings form. |
| **Authorization** | `[Authorize]` |
| **Inputs** | None. |
| **Response** | Renders `Views/Settings/Index.cshtml` with `CompanySettingsViewModel`. If `CompanyId` is null → `Challenge()`. If service fails → error view. |
| **Validation** | **Service (`ICompanySettingsService.GetAsync`):** `companyId` > 0; returns company fields plus `PrimaryColor` and `InvoiceTerms` from `CompanySetting` rows, with defaults (`#1F2937` and a generic invoice-terms sentence) when settings are missing. |

### `POST /Settings` (or `/Settings/Index`)

| | |
|---|---|
| **Purpose** | Save company profile and settings changes. |
| **Authorization** | `[Authorize]` |
| **Inputs** | Form fields: `CompanyName`, `Address`, `City`, `State`, `Country`, `PinCode`, `GstNumber`, `PanNumber`, `Mobile`, `Email`, `Website`, `PrimaryColor`, `InvoiceTerms`. Anti-forgery token required. |
| **Response** | On success → redirect to `/Settings/Index` with `TempData["SuccessMessage"]`. On failure → re-render form with errors. |
| **Validation** | **Model binding / data annotations:** `CompanyName`, `Address`, `City`, `State`, `Country`, `PinCode`, `GstNumber`, `PanNumber`, `Mobile`, `Email`, `PrimaryColor`, `InvoiceTerms` — all required; `Email` must be valid email format; `Website` is optional. **Service (`ICompanySettingsService.UpdateAsync`):** `companyId` > 0; company must exist; string fields trimmed on save; `PrimaryColor` and `InvoiceTerms` upserted as `CompanySetting` rows. |

---

## SearchController

**Class-level:** `[Authorize]`  
**Services:** `ICurrentCompanyService`, `ISearchService`

### `GET /Search` (or `/Search/Index`)

| | |
|---|---|
| **Purpose** | Global search across products, customers, and quotations for the current company. |
| **Authorization** | `[Authorize]` |
| **Inputs** | Query: `keyword` (optional) — search term submitted from the navbar search form. |
| **Response** | Renders `Views/Search/Index.cshtml` with `SearchIndexViewModel` (`Keyword`, `Results` as `GlobalSearchResultDto`). If `CompanyId` is null → `Challenge()`. If service fails → error view. |
| **Validation** | **Service (`ISearchService.SearchAsync`):** `companyId` > 0; keyword trimmed. If keyword length < 2 characters, returns empty results with `KeywordTooShort = true` when keyword is non-empty (view shows a friendly message). When keyword ≥ 2 characters, searches: **Products** — `Name`, `Sku`, `Barcode`; **Customers** — `Name`, `Code`; **Quotations** — `QuotationNumber`, linked customer `Name`. All results scoped to company; each result includes a URL to the relevant MVC page. |

---

## Application Services Reference

| Controller action | Application / infrastructure service | Method |
|-------------------|-------------------------------------|--------|
| Dashboard `Index` | `IDashboardService` | `GetSummaryAsync(companyId)` |
| Products `Index` | `IProductService` | `SearchAsync(companyId, search)` |
| Customers `Index` | `ICustomerService` | `SearchAsync(companyId, search)` |
| Quotations `Index` | `IQuotationService` | `GetListAsync(companyId)` |
| Quotations `Create` (GET) | `ICustomerService`, `IProductService` | `SearchAsync(companyId, null)` |
| Quotations `Create` (POST) | `IQuotationService` | `CreateAsync(companyId, request)` |
| Quotations `Details` | `IQuotationService` | `GetDetailAsync(companyId, id)` |
| Quotations `DownloadPdf` | `IQuotationService`, `ICompanySettingsService`, `IQuotationPdfService` | `GetDetailAsync`, `GetAsync`, `GeneratePdf` |
| Settings `Index` (GET/POST) | `ICompanySettingsService` | `GetAsync`, `UpdateAsync` |
| Search `Index` | `ISearchService` | `SearchAsync(companyId, keyword)` |
| All protected actions | `ICurrentCompanyService` | `GetCompanyIdAsync()` |

All Application services return `ServiceResult<T>` (or `ServiceResult`) for expected failures rather than throwing for validation errors.

---

## Response Types Summary

| Response type | Used by |
|---------------|---------|
| Razor view (HTML) | All `Index`, `Create`, `Details`, `Login` actions |
| Redirect | Successful login, logout, quotation create, settings save |
| `Challenge()` | Authenticated user with no resolvable `CompanyId` |
| `NotFound()` | Quotation detail/PDF when quotation missing or cross-tenant |
| `File()` (PDF download) | `Quotations/DownloadPdf` only |
| Error view | Service failure on dashboard, list, settings, search, PDF settings load |

There are **no JSON responses** and **no AJAX endpoints** in the current implementation.
