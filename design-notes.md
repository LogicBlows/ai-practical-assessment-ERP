# SmeErp Design Notes

## 1. Architecture Overview

SmeErp follows **Clean Architecture** with five projects under `src/`:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Domain | `SmeErp.Domain` | Entities and domain types only; **no external dependencies** |
| Application | `SmeErp.Application` | DTOs, service interfaces, `ServiceResult<T>`, and pure business logic (e.g. `QuotationTotalsCalculator`) |
| Infrastructure | `SmeErp.Infrastructure` | EF Core `AppDbContext`, migrations, Fluent API configuration, ASP.NET Identity (`ApplicationUser`), and service implementations |
| Shared | `SmeErp.Shared` | Cross-cutting constants (e.g. `CompanySettingKeys`) |
| Presentation | `SmeErp.Web` | ASP.NET Core MVC — controllers, Razor views, view models, view components, static assets |

**Dependency direction:** `Web` → `Application`, `Infrastructure`, `Shared`; `Infrastructure` → `Application`, `Domain`, `Shared`; `Application` → `Domain`, `Shared`. `Domain` depends on nothing.

### Why this structure

- **Separation of concerns** — each layer has a single reason to change. Domain entities do not know about EF Core or HTTP. The web layer does not know about SQL or DbContext.
- **No business logic in controllers or views** — controllers resolve the current company, call an Application-layer service, map the `ServiceResult<T>` to a view/redirect, and return. Razor views display data and submit forms; they do not calculate totals, validate tenant ownership, or query the database.
- **No direct `DbContext` access outside Infrastructure** — only Infrastructure service classes (and seeders) use `AppDbContext`. Controllers inject `IProductService`, `IQuotationService`, etc., never `AppDbContext`.
- **Testability** — pure calculation logic (`QuotationTotalsCalculator`) lives in Application and is unit-tested without a database. Service interfaces allow mocking at the web boundary.

### Authentication and signing key (current state)

- **ASP.NET Identity** provides cookie-based authentication. `ApplicationUser` extends `IdentityUser` with `FullName` and `CompanyId`. Users are seeded at startup; there is no self-registration flow.
- **`ISigningKeyService`** generates and stores signing keys in the `SigningKeys` table at runtime (`RandomNumberGenerator`, 30-day expiry, rotation support). A startup seeder ensures an active key exists. **JWT token generation/validation is not wired yet** — the service and table are infrastructure in place for a future API or claims layer.

---

## 2. Frontend Design

SmeErp uses **server-rendered Razor MVC** — no SPA framework, no client-side API calls.

### Theming and layout

- **Bootswatch Flatly** (Bootstrap 5.1.3) is loaded via CDN in `_Layout.cshtml`, replacing the default Bootstrap stylesheet.
- **Card-based layouts** (`page-card` in `site.css`) wrap Dashboard, Settings, and Quotation Create/Details pages.
- **Striped/hover tables** (`table-striped`, `table-hover`) on Products, Customers, and Quotations list pages.

### Settings-driven branding

Company appearance is driven by the `PrimaryColor` setting (key `CompanySettingKeys.PrimaryColor`):

1. `CompanySettingsService` reads/writes `PrimaryColor` (and `InvoiceTerms`) as `CompanySetting` rows, with a default of `#1F2937` when missing.
2. **`CompanyBrandingViewComponent`** runs on every page load via `_Layout.cshtml`. For authenticated users it calls `ICompanySettingsService.GetAsync` and injects a CSS variable `--company-navbar-bg` from `PrimaryColor`.
3. The navbar uses `company-navbar` styling tied to that variable; the brand title shows the company name.
4. **`QuotationPdfService`** (QuestPDF) reads the same `CompanySettingsDto.PrimaryColor` for PDF header/totals accent color, keeping UI and PDF branding consistent after settings changes.

Settings are loaded fresh on each PDF download (no caching), so a settings change is reflected on the next PDF generation.

### Navigation

Authenticated users see Dashboard, Products, Customers, Quotations, Settings, and a global search form (GET to `/Search`). Unauthenticated users see Home and Sign in.

---

## 3. Backend Design

### Application-layer services

Business operations are defined as interfaces in `SmeErp.Application.Interfaces.Services` and implemented in `SmeErp.Infrastructure.Services`:

| Service | Responsibility |
|---------|----------------|
| `IProductService` | Company-scoped product search |
| `ICustomerService` | Company-scoped customer search |
| `IQuotationService` | Create, list, and detail quotations |
| `ICompanySettingsService` | Read/update company profile and settings |
| `IQuotationPdfService` | Generate quotation PDF bytes from DTOs |
| `ISearchService` | Global search across products, customers, quotations |
| `IDashboardService` | KPI counts for dashboard cards |
| `ICurrentCompanyService` | Resolve logged-in user's `CompanyId` |
| `ISigningKeyService` | Get/rotate DB-stored signing keys |

All services that perform business operations return **`ServiceResult<T>`** (or non-generic `ServiceResult`) for success/failure instead of throwing exceptions for expected validation errors.

### Controllers as thin orchestrators

Controllers follow a consistent pattern:

1. Resolve `companyId` via `ICurrentCompanyService.GetCompanyIdAsync()`; return `Challenge()` if null.
2. Call the appropriate Application service with `companyId` and request DTOs/view-model data.
3. On `result.Succeeded` — pass `result.Data` to a Razor view or redirect.
4. On failure — add `result.Error` to `ModelState`, return `View("Error")`, or `NotFound()` depending on context.

Controllers may perform **presentation-level** checks (e.g. stripping empty quotation line rows, requiring at least one line before calling the service) but delegate **business rules** (customer belongs to company, product validity, quantity > 0, totals calculation) to services.

### DTOs and view models

- **DTOs** (`SmeErp.Application.DTOs`) cross the Application/Infrastructure boundary and carry data between services and controllers.
- **View models** (`SmeErp.Web.Models`) add display metadata (`[Display]`, `[Required]`) for Razor forms. Controllers map between view models and DTOs at the edge (e.g. `CreateQuotationViewModel` → `CreateQuotationRequestDto`, `CompanySettingsViewModel` ↔ `CompanySettingsDto`).

---

## 4. Database Design

### SQL Server via EF Core 6

- **`AppDbContext`** extends `IdentityDbContext<ApplicationUser>` and exposes `DbSet<T>` for all domain entities plus Identity tables.
- **Code-first migrations** live in `SmeErp.Infrastructure/Persistence/Migrations`. Schema is configured via Fluent API classes in `Persistence/Configurations/` (field lengths, decimal precision, relationships, indexes, seed data).
- Connection string is read from `appsettings.json` → `ConnectionStrings:DefaultConnection`.

### Multi-tenant isolation

- **`Company`** is the tenant root (no `CompanyId`).
- Tenant-scoped entities carry **`CompanyId`**: `CompanySetting`, `Product`, `Customer`, `Quotation`, and `ApplicationUser`.
- **`QuotationLine`** has no `CompanyId`; isolation is enforced through its parent `Quotation`.
- **`SigningKey`** is global (application-wide, not per company).

At request time, `ICurrentCompanyService` reads the authenticated user's `CompanyId`. Every service method accepts `companyId` as an explicit parameter and filters queries with `WHERE CompanyId = @companyId` (or equivalent join/filter). Cross-tenant access returns failure or `NotFound`, not another company's data.

### Indexing strategy

`CompanyId` is indexed on every entity that has it (`CompanySetting`, `Product`, `Customer`, `Quotation`, `ApplicationUser`) to support efficient tenant-filtered queries.

`SigningKey` is indexed on `IsActive` to quickly locate the current active key.

### Delete behaviors

- **`Restrict`** on `Company` → child entities (prevents accidental cascade deletion of a tenant's data).
- **`Cascade`** on `Quotation` → `QuotationLine` (lines are removed with their parent quotation).
- **`Restrict`** on `QuotationLine` → `Product` (products cannot be deleted while referenced).

### Seed data

Fixed reference data (companies, products, customers, settings) is seeded via EF `HasData()` in migrations. Users, roles, and signing keys are seeded at **runtime** (`IdentitySeeder`, `SigningKeySeeder`) on application startup.

---

## 5. Validation Strategy

Validation is split by responsibility:

### Application / Infrastructure services (business validation)

Expected business-rule failures are validated inside service implementations and returned as `ServiceResult.Failure("message")`. Examples from `QuotationService`:

- `companyId` must be > 0
- `CustomerId` must exist and belong to the company
- At least one line item required
- Each line `Quantity` must be > 0
- Each `ProductId` must belong to the company
- GST percent is taken from the product record, not user input

Similar checks exist in other services (`companyId > 0`, company must exist for settings updates, search keyword minimum length of 2 characters in `SearchService`).

**Business logic does not live in Razor views.** Views render validation messages from `ModelState` but do not enforce rules themselves.

### Web layer (input / form validation)

View models use **data annotations** for structural input validation before a service is called:

- `LoginViewModel` — required email (valid format), required password
- `CreateQuotationViewModel` — required customer (range ≥ 1), required dates
- `CompanySettingsViewModel` — required fields, valid email on company email

Controllers check `ModelState.IsValid` on POST actions and re-render the form with errors if invalid. Controllers may add additional presentation rules (e.g. at least one non-empty line item on quotation create).

### ASP.NET Identity (credential policy)

Password complexity is configured in `Program.cs` (digit, lower, upper, non-alphanumeric, minimum length 8). This applies when users are created via `UserManager`, not on every login attempt.

### Pure calculation (Application layer, no I/O)

`QuotationTotalsCalculator` in `SmeErp.Application/Services/` performs line and quotation total math. It is called from `QuotationService` and covered by unit tests, keeping calculation logic independent of EF Core.

---

## 6. Error Handling Strategy

SmeErp uses **explicit failure results** rather than exceptions for expected error paths.

### ServiceResult pattern

```csharp
// Success
ServiceResult<T>.Success(data)

// Expected failure (validation, not found, etc.)
ServiceResult<T>.Failure("Human-readable error message")
```

Controllers inspect `result.Succeeded`:

| Scenario | Controller behavior |
|----------|---------------------|
| Service failure on form POST | `ModelState.AddModelError(string.Empty, result.Error)` → re-render form |
| Service failure on read (list/dashboard/settings/search) | `return View("Error")` |
| Quotation not found / wrong tenant | `return NotFound()` |
| No resolvable `CompanyId` | `return Challenge()` → redirect to login |
| Login failure | `ModelState` error: "Invalid email or password." |
| Settings save success | `TempData["SuccessMessage"]` + redirect to Index |

Unexpected exceptions in non-Development environments are caught by `UseExceptionHandler("/Home/Error")` in `Program.cs`, which renders the generic `Home/Error` view with a request ID.

### What is not thrown for expected errors

Services do not throw for validation failures, missing entities, or cross-tenant mismatches — they return `ServiceResult.Failure(...)`. This keeps controller error handling predictable and avoids try/catch around every service call for business rules.

### PDF and file responses

`QuotationsController.DownloadPdf` returns `File(pdfBytes, "application/pdf", fileName)` on success. If the quotation is missing it returns `NotFound()`; if settings cannot be loaded it falls back to the error view.

---

## 7. Key Integration Points

| Concern | How it connects |
|---------|-----------------|
| **Identity → tenancy** | `ApplicationUser.CompanyId` links each user to one `Company`; `ICurrentCompanyService` exposes it per request |
| **Settings → UI** | `CompanyBrandingViewComponent` reads `PrimaryColor` for navbar CSS variable |
| **Settings → PDF** | `DownloadPdf` loads `CompanySettingsDto` alongside `QuotationDetailDto` and passes both to `IQuotationPdfService` |
| **Products → quotations** | `QuotationService` reads product `GstPercent` at creation time; line totals calculated via `QuotationTotalsCalculator` |
| **Signing key** | `SigningKeySeeder` calls `GetActiveKeyAsync()` at startup; key stored in DB, not in config or source code |

---

## Related Documentation

- [README.md](README.md) — setup and run instructions
- [data-model.md](data-model.md) — entity fields, relationships, and indexing
- [api-contract.md](api-contract.md) — MVC controller actions and inputs/responses
