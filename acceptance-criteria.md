# SmeErp Acceptance Criteria (Option 3 — .NET Full-Stack SME ERP)

Cross-referenced against the implemented codebase (`SmeErp.Web` controllers/views, Application/Infrastructure services, EF Core migrations, xUnit tests, `debugging-notes.md`, and `ai-prompts/planning.md` manual verification notes).

---

## Core

- [x] A user can log in with seeded credentials  
  **Evidence:** `IdentitySeeder` creates `admin@sharmatrading.com` and `admin@vermadist.com` with demo password; `AccountController` uses `SignInManager.PasswordSignInAsync`. Manually verified in Prompt #6 (dashboard showed correct `CompanyId` per user) and `debugging-notes.md` Issue 1.

- [x] A user can list and search products from the database  
  **Evidence:** `ProductsController.Index` → `IProductService.SearchAsync` filters by `CompanyId` and optional keyword (name, SKU, barcode). Manually verified in Prompt #8 (Sharma sees 4 hardware products; Verma sees 4 stationery products).

- [x] A user can list customers from the database  
  **Evidence:** `CustomersController.Index` → `ICustomerService.SearchAsync` filters by `CompanyId` (with optional name/code search). Manually verified in Prompt #8 (3 customers per company, tenant-scoped).

- [x] A user can create a quotation with multiple line items via the UI  
  **Evidence:** `Quotations/Create.cshtml` supports repeatable line items; `POST Quotations/Create` → `IQuotationService.CreateAsync`. Manually verified in Prompt #9 (hand-calculated totals matched UI output on detail view; quotation `QT-1-00002` referenced in `test-results.md`).

- [x] A user can view the quotation list and open a detail view  
  **Evidence:** `QuotationsController.Index` (`GetListAsync`) and `Details` (`GetDetailAsync`); list has **View** links to detail. Implemented and used during Prompt #9 verification.

- [x] A user can download/print a quotation PDF  
  **Evidence:** `QuotationsController.DownloadPdf` streams `application/pdf` via `IQuotationPdfService.GeneratePdf`. Manually verified in `debugging-notes.md` Issues 3 and 5 (PDF generates, streams, and renders correctly after font fix).

- [x] PDF company address, GSTIN, terms, and accent color come from Settings (not hardcoded)  
  **Evidence:** `DownloadPdf` loads `CompanySettingsDto` via `ICompanySettingsService.GetAsync` on each request; `QuotationPdfService` uses company name, address, GSTIN (`GstNumber`), PAN, contact info, `InvoiceTerms`, and `PrimaryColor` from that DTO.

- [x] Changing Settings updates the next PDF output  
  **Evidence:** No PDF caching — settings and quotation are loaded fresh per download. Manually verified in Prompt #10 and re-confirmed in `debugging-notes.md` Issue 3 (address and `PrimaryColor` changes reflected on next PDF).

- [x] Global search returns relevant products and customers (and quotations if implemented)  
  **Evidence:** `SearchController` → `ISearchService.SearchAsync` returns grouped Products, Customers, and Quotations (minimum 2-character keyword). Manually verified in `debugging-notes.md` Issue 4 (search for "Havells" returned product; cross-tenant isolation confirmed).

- [x] Data persists after application restart  
  **Evidence:** Data stored in SQL Server via EF Core migrations (`AppDbContext`); not in-memory at runtime. Schema and seed data applied via `dotnet ef database update`. Persistence is inherent to the SQL Server design; quotations created in UI are saved with `SaveChangesAsync` in `QuotationService`.

- [x] Backend validation rejects invalid quotations (missing customer, zero quantity, etc.)  
  **Evidence:** `QuotationService.CreateAsync` returns `ServiceResult.Failure` for invalid `companyId`, customer not in company, no line items, `Quantity <= 0`, and invalid/cross-tenant `ProductId`. Controller surfaces `result.Error` via `ModelState`. Rules are implemented in service code; rejection paths are not covered by automated tests (happy-path UI creation is manually verified).

- [x] UI shows validation and error states clearly  
  **Evidence:** Razor views use `asp-validation-summary`, `asp-validation-for`, and `ModelState` errors (login invalid credentials, quotation create failures, settings save failures). `[Authorize]` redirects unauthenticated users to `/Account/Login`. Service failures on reads return the shared Error view; missing quotations return `404`.

- [x] No secrets committed to the repository  
  **Evidence:** JWT signing keys generated at runtime via `RandomNumberGenerator` in `SigningKeyService` (not in config/source). `appsettings.json` uses a local SQL Server placeholder with `Trusted_Connection` (no SQL password). Demo user password in `IdentitySeeder` is explicitly marked local/demo-only and is not a production credential. No API keys or hardcoded signing secrets in source.

- [x] README setup instructions work on a clean machine  
  **Evidence:** `README.md` documents clone → connection string → `dotnet ef database update` → `dotnet run` → login → `dotnet test`. Manually followed step-by-step on a fresh check and confirmed working (documented in `ai-prompts/documentation.md` Prompt #9).

- [x] Mandatory xUnit tests pass  
  **Evidence:** `dotnet test` — **2 passed, 0 failed** (verified 2026-07-24). Results also recorded in `test-results.md` (2026-07-21, 2/2 passing).

---

## Validation

Validation rules **actually implemented** in the codebase:

### Quotation creation (`QuotationService.CreateAsync` + controller)

| Rule | Where enforced |
|------|----------------|
| `companyId` must be > 0 | `QuotationService` |
| `CustomerId` must exist and belong to the user's company | `QuotationService` |
| At least one line item required | `QuotationService`; controller also strips lines with `ProductId <= 0` and requires ≥ 1 remaining line |
| Each line `Quantity` must be > 0 | `QuotationService` |
| Each `ProductId` must belong to the user's company | `QuotationService` |
| GST percent taken from product record (not user input) | `QuotationService` / `QuotationTotalsCalculator` |
| Quotation dates stored as date-only (`.Date`) | `QuotationService` |

### Quotation create form (`CreateQuotationViewModel` data annotations)

| Rule | Where enforced |
|------|----------------|
| `CustomerId` required, must be ≥ 1 | Model binding |
| `QuotationDate` required | Model binding |
| `ValidUntil` required | Model binding |

### Login (`LoginViewModel`)

| Rule | Where enforced |
|------|----------------|
| `Email` required, valid email format | Model binding |
| `Password` required | Model binding |

### Company settings (`CompanySettingsViewModel`)

| Rule | Where enforced |
|------|----------------|
| Required fields: company name, address, city, state, country, PIN, GST, PAN, mobile, email, primary color, invoice terms | Model binding |
| `Email` must be valid email format | Model binding |
| `Website` optional | Model binding |

### Global search (`SearchService`)

| Rule | Where enforced |
|------|----------------|
| Keyword must be ≥ 2 characters to run search | `SearchService` (shorter keywords return empty results with `KeywordTooShort` flag) |
| All matches scoped to `companyId` | `SearchService` |

### ASP.NET Identity (user creation policy)

| Rule | Where enforced |
|------|----------------|
| Password: digit, lower, upper, non-alphanumeric, min length 8 | `Program.cs` Identity options |

### Other services

| Rule | Where enforced |
|------|----------------|
| `companyId` must be > 0 on read operations | `ProductService`, `CustomerService`, `DashboardService`, `CompanySettingsService`, `SearchService` |
| Company must exist for settings get/update | `CompanySettingsService` |
| Quotation detail: must match `id` and `companyId` | `QuotationService.GetDetailAsync` |

---

## Error Handling

Failure states **actually implemented**:

| Scenario | Behavior |
|----------|----------|
| Expected business/validation failure | `ServiceResult.Failure("message")` → controller adds to `ModelState` (POST) or returns Error view / `NotFound()` (GET) |
| Invalid login credentials | `ModelState` error: "Invalid email or password." |
| Quotation not found or wrong tenant | `NotFound()` on Details and DownloadPdf |
| Authenticated user with no resolvable `CompanyId` | `Challenge()` → redirect to login |
| Unauthenticated access to `[Authorize]` controller | Redirect to `/Account/Login` (cookie auth) |
| Service failure on dashboard/list/settings/search read | `return View("Error")` |
| Settings save failure | `ModelState` error with service message; re-render form |
| Unhandled exception (non-Development) | `UseExceptionHandler("/Home/Error")` in `Program.cs` |
| PDF settings load failure | Error view (DownloadPdf) |

Controllers do **not** catch exceptions around service calls for expected validation failures — they check `result.Succeeded`.

---

## Testing

### What is tested (automated)

| Test | File | What it covers |
|------|------|----------------|
| `Calculate_WithKnownLineItem_ProducesExpectedTotals` | `QuotationCalculationTests.cs` | `QuotationTotalsCalculator` math (SubTotal, discount, tax, total for known inputs) |
| `GetAsync_WhenCompanyHasNoSettings_ReturnsDocumentedDefaults` | `CompanySettingsDefaultsTests.cs` | Default `PrimaryColor` and `InvoiceTerms` when no `CompanySetting` rows exist |

**Current status:** `dotnet test` — 2/2 passing.

### What is NOT tested (gaps)

| Gap | Notes |
|-----|-------|
| Integration / end-to-end tests | No WebApplicationFactory or browser tests |
| `QuotationService.CreateAsync` validation rejection paths | Invalid customer, zero quantity, wrong product — implemented but not unit tested |
| Controllers | No controller tests |
| PDF generation | No automated test for `QuotationPdfService` output |
| Global search | No unit tests for `SearchService` |
| Multi-tenant isolation | Manually verified during development, not automated |
| Login / Identity flows | Manually verified, not automated |
| Settings update (`UpdateAsync`) | Not unit tested (only defaults on get are tested) |
| Edge cases | Empty search, concurrent users, expired signing keys, etc. |

Manual verification during development is documented in `ai-prompts/planning.md` (Prompts #6–#13) and `debugging-notes.md` (Issues 1–6).

---

## Documentation

Lifecycle documents **complete as of this point**:

| Document | Status | Location |
|----------|--------|----------|
| README (setup, credentials, tests, structure) | Complete | `README.md` |
| Data model | Complete | `data-model.md` |
| API / MVC contract | Complete | `api-contract.md` |
| Design notes | Complete | `design-notes.md` |
| UI flow | Complete | `ui-flow.md` |
| Test results | Complete | `test-results.md` |
| Debugging notes | Complete | `debugging-notes.md` |
| AI planning log | Complete | `ai-prompts/planning.md` (Prompts #1–#14) |
| AI documentation log | Complete | `ai-prompts/documentation.md` (Prompts #1–#13) |

### Not present

| Document | Notes |
|----------|-------|
| `test-strategy.md` | Referenced in `test-results.md` but file does not exist in the repository |
| Formal assessment requirements file | Option 3 criteria were provided in the session prompt; no separate requirements file in the repo |
