# PR Description

Consolidated summary of the seven merged feature pull requests that delivered SmeErp Core scope. GitHub CLI was not available in the documentation environment; PR content is reconstructed from merge commits, branch names, `ai-prompts/planning.md` entries, `debugging-notes.md`, and `test-results.md`.

---

## Overview

SmeErp (Option 3 — .NET Full-Stack SME ERP) was built incrementally across **seven sequential feature branches**, each merged via an independent pull request after manual verification. Every PR was tested in the browser (and SSMS where relevant) before merge; several issues found during review were fixed within the same PR or documented in `debugging-notes.md`.

| PR | Branch | Primary scope |
|----|--------|---------------|
| #1 | `feature/auth-and-roles` | Authentication, roles, current-company resolution, DB-stored JWT signing key |
| #2 | `feature/products-customers` | Products and Customers list + search |
| #3 | `feature/quotations` | Quotation create, list, and detail |
| #4 | `feature/settings-and-pdf` | Company settings and quotation PDF generation |
| #5 | `feature/ui-styling` | Bootswatch theme and card-based UI styling |
| #6 | `feature/search-and-dashboard` | Global search and dashboard KPI cards |
| #7 | `feature/tests` | Mandatory xUnit tests |

Merge commits: `fa40b17`, `282a878`, `c8f1781`, `7251878`, `09188b5`, `84d1460`, `83c3451`.

---

## PR #1: Add authentication, roles, and DB-stored JWT signing key

**Branch:** `feature/auth-and-roles`  
**Merge:** `fa40b17` — Merge pull request #1  
**Planning prompts:** #6 (auth), #7 (signing key)  
**Key commits:** `e73ccec` Add auth, seeded users/roles, and current-company resolution (prompt #6); `d46e8eb` Add DB-stored JWT signing key generation and rotation (prompt #7)

### Summary

Introduced ASP.NET Identity cookie authentication, seeded roles and demo users linked to companies, per-request tenant resolution via `ICurrentCompanyService`, and infrastructure for runtime-generated signing keys stored in the database (not wired to JWT auth yet).

### Features Implemented

- Login and logout via email/password (`AccountController`, Razor login view)
- Seeded roles: `Admin`, `Proprietor`
- Seeded users: `admin@sharmatrading.com` (Company 1), `admin@vermadist.com` (Company 2), both `Proprietor`
- `[Authorize]` placeholder `DashboardController` redirecting unauthenticated users to login
- `ICurrentCompanyService` resolving logged-in user's `CompanyId`
- `SigningKey` entity, `ISigningKeyService` / `SigningKeyService` with `GetActiveKeyAsync` and `RotateKeyAsync`
- Startup seeders: `IdentitySeeder`, `SigningKeySeeder`

### Technical Changes

- Extended `ApplicationUser` with `FullName` and `CompanyId`
- Migration `20260719192330_SeedUsersAndRoles`
- Migration `20260720054641_AddSigningKeyTable`
- Identity and signing key services registered in `Program.cs`
- Signing keys generated via `RandomNumberGenerator` (64 bytes, base64), 30-day expiry — no hardcoded secrets

### Testing Done

- Logged in as both seeded users; dashboard showed correct company ID (1 and 2) per account
- Queried `SigningKeys` in SSMS: one active, non-hardcoded key with correct expiry
- Resolved port-conflict and DLL-lock issues from stale `dotnet` processes (`debugging-notes.md` Issues 1–2)

### Known Limitations

- No self-registration; users are seeded only
- JWT signing key not connected to token generation/validation
- No role-based authorization on controllers (`[Authorize]` only)
- Dashboard was a placeholder (company ID debug text) until PR #6

---

## PR #2: Add Products and Customers list/search pages

**Branch:** `feature/products-customers`  
**Merge:** `282a878` — Merge pull request #2  
**Planning prompt:** #8  
**Key commit:** `e7b2fab` Add Products and Customers list/search pages (prompt #8)

### Summary

Added company-scoped product and customer list pages with keyword search, following Clean Architecture (Application services, DTOs, thin controllers).

### Features Implemented

- `ProductsController.Index` — list and search by name, SKU, or barcode
- `CustomersController.Index` — list and search by name or code
- Nav links for Products and Customers (authenticated users only)
- `ServiceResult<T>` pattern with `IProductService` / `ICustomerService`

### Technical Changes

- DTOs: `ProductListItemDto`, `CustomerListItemDto`
- `ProductService` / `CustomerService` in Infrastructure (EF queries filtered by `CompanyId`)
- Razor `Index.cshtml` views with GET search forms and result tables
- `[Authorize]` on both controllers

### Testing Done

- `dotnet build SmeErp.sln` — 0 warnings
- Manual: Sharma Trading sees 4 hardware products and 3 Jaipur customers
- Manual: Verma Distributors sees 4 stationery products and 3 Pune customers (tenant isolation confirmed)

### Known Limitations

- List and search only — no create, edit, or delete UI for products/customers
- Products and customers remain migration-seeded reference data

---

## PR #3: Add Quotation creation, list, and detail flow

**Branch:** `feature/quotations`  
**Merge:** `c8f1781` — Merge pull request #3  
**Planning prompt:** #9  
**Key commit:** `d799cf2` Add Quotation creation, list, and detail flow (prompt #9)

### Summary

Delivered the core quotation workflow: create with multiple line items, company-scoped validation, automatic totals and quotation numbering, list view, and read-only detail view.

### Features Implemented

- Quotation list (newest first)
- Create form with customer selection, dates, notes, repeatable line items (plain JS add/remove, product price auto-fill)
- Detail view with line items and totals breakdown
- Auto-generated quotation numbers: `QT-{CompanyId}-{sequential}`
- Per-line discount and GST calculations (GST % from product)

### Technical Changes

- DTOs: `QuotationLineInputDto`, `CreateQuotationRequestDto`, `QuotationListItemDto`, `QuotationLineDetailDto`, `QuotationDetailDto`
- `IQuotationService` / `QuotationService`: `CreateAsync`, `GetListAsync`, `GetDetailAsync`
- `QuotationsController` with Index, Create (GET/POST), Details
- Nav link for Quotations

### Testing Done

- `dotnet build` — 0 warnings
- Hand-calculated line and quotation totals matched UI detail view output
- Cross-tenant isolation: Verma's user sees zero of Sharma's quotations

### Known Limitations

- No quotation edit or delete after creation
- No PDF download (added in PR #4)
- Validation rejection paths not covered by automated tests

---

## PR #4: Add Company Settings page and Quotation PDF generation

**Branch:** `feature/settings-and-pdf`  
**Merge:** `7251878` — Merge pull request #4  
**Planning prompt:** #10  
**Key commit:** `8e2f3b5` Add Company Settings page and Quotation PDF generation with font fix (prompt #10)

### Summary

Added company profile and branding settings (`PrimaryColor`, `InvoiceTerms`) and QuestPDF-based quotation PDF download branded from those settings. Fixed a font ligature bug that corrupted "ti" sequences in PDF text.

### Features Implemented

- Settings page (GET/POST) for company profile, color picker, invoice terms
- `ICompanySettingsService` with defaults when settings rows missing
- PDF download from quotation detail (`DownloadPdf` action)
- Branded PDF: company header, GSTIN, PAN, line items, totals, footer terms, `PrimaryColor` accent
- Settings loaded fresh on each PDF request (no caching)

### Technical Changes

- `CompanySettingsDto`, `CompanySettingKeys` in Shared
- `CompanySettingsService` with get/update and setting upsert
- QuestPDF 2024.12.3; `LicenseType.Community` in `Program.cs`
- `IQuotationPdfService` / `QuotationPdfService`
- `QuotationDetailDto` extended with customer address fields for PDF
- Font fix: explicit `FontFamily("Arial")` and disabled standard ligatures in `QuotationPdfService` (`debugging-notes.md` Issue 3)

### Testing Done

- Changed address and `PrimaryColor` in Settings; next PDF download reflected updated values
- Verified PDF text renders correctly after font fix (no "Quotaon" corruption)
- PDF streams as `application/pdf` with filename `{QuotationNumber}.pdf`

### Known Limitations

- `IQuotationPdfService` DI registration was initially omitted and fixed later in PR #6 (`debugging-notes.md` Issue 5)
- No PDF snapshot/automated tests
- Logo path field exists on `Company` entity but is not used in PDF UI

---

## PR #5: Apply Bootswatch theme and card-based UI styling

**Branch:** `feature/ui-styling`  
**Merge:** `09188b5` — Merge pull request #5  
**Planning prompt:** #11  
**Key commit:** `5425453` Apply Bootswatch theme and card-based layout styling pass

### Summary

Visual-only styling pass: Bootswatch Flatly theme, settings-driven navbar branding, striped/hover tables, and card layouts — no controller or route changes.

### Features Implemented

- Bootswatch Flatly (Bootstrap 5.1.3) via CDN in `_Layout.cshtml`
- `CompanyBrandingViewComponent` — injects `PrimaryColor` as navbar CSS variable and company name as brand title
- Striped/hover tables on Products, Customers, Quotations lists
- Card layouts on Dashboard, Settings, Quotation Create/Details

### Technical Changes

- `CompanyBrandingViewComponent` and view
- `site.css` updates (`page-card`, page background, navbar styling)
- Removed conflicting local Bootstrap CSS link

### Testing Done

- Manual regression: login, search, quotation creation, and PDF download all confirmed working after visual-only change
- No functional or route changes verified by re-testing all major flows

### Known Limitations

- Theme loaded from CDN (requires network for first load)
- Unauthenticated Home page retains default welcome template styling

---

## PR #6: Add Global Search and Dashboard KPI cards

**Branch:** `feature/search-and-dashboard`  
**Merge:** `84d1460` — Merge pull request #6  
**Planning prompts:** #12 (search), #13 (dashboard)  
**Key commit:** `7c20d79` Add Global Search and Dashboard KPI cards (prompts #12 and #13)

### Summary

Added navbar global search with grouped results and a dashboard with four company-scoped KPI cards. Fixed DbContext concurrency bugs in `SearchService` and `DashboardService`, and restored missing `IQuotationPdfService` DI registration discovered during verification.

### Features Implemented

- Global search (products, customers, quotations) — minimum 2-character keyword
- Navbar search form → `/Search` results grouped by type with links
- Dashboard KPI cards: Total Products, Total Customers, Quotations Today, Pending Quotations
- Removed placeholder "Current company ID" debug text from dashboard

### Technical Changes

- `ISearchService` / `SearchService`, `SearchController`, `Search/Index.cshtml`
- DTOs: `GlobalSearchResultDto`, `SearchResultItemDto`, `SearchResultType`
- `IDashboardService` / `DashboardService`, `DashboardSummaryDto`
- **Fix:** Replaced `Task.WhenAll` with sequential `await` in `SearchService` and `DashboardService` (`debugging-notes.md` Issues 4, 6)
- **Fix:** Added `AddScoped<IQuotationPdfService, QuotationPdfService>()` in `Program.cs` (Issue 5)

### Testing Done

- Search for "Havells" returned Sharma product without exception
- Dashboard loads for both users with correct, different counts:
  - Sharma: 4 products, 3 customers, 3 quotations today, 3 pending
  - Verma: 4 products, 3 customers, 0 quotations today, 0 pending
- Cross-tenant isolation re-verified for search results
- Quotation Details and PDF download confirmed working after DI fix

### Known Limitations

- Full page reload on search (no autocomplete/AJAX)
- Sequential DB queries (concurrency-safe but not parallelized)
- No charts on dashboard (Stretch tier)

---

## PR #7: Add mandatory xUnit tests

**Branch:** `feature/tests`  
**Merge:** `83c3451` — Merge pull request #7  
**Planning prompt:** #14  
**Key commits:** `767cbdc` Add mandatory xUnit tests; `27559f8` Record mandatory xUnit test results

### Summary

Added the two mandatory assessment xUnit tests: quotation calculation correctness and company settings defaults when no setting rows exist.

### Features Implemented

- `QuotationCalculationTests.Calculate_WithKnownLineItem_ProducesExpectedTotals`
- `CompanySettingsDefaultsTests.GetAsync_WhenCompanyHasNoSettings_ReturnsDocumentedDefaults`
- `QuotationTotalsCalculator` extracted to Application layer for testability

### Technical Changes

- `QuotationTotalsCalculator`, `QuotationLineCalculationInput`, result types in `SmeErp.Application/Services/`
- `QuotationService` refactored to delegate calculations to `QuotationTotalsCalculator`
- `Microsoft.EntityFrameworkCore.InMemory` 6.0.36 added to test project
- Test project references Application and Infrastructure
- Results recorded in `test-results.md`

### Testing Done

- `dotnet test` — **2 passed, 0 failed** (3.6s on 2026-07-21)
- Quotation test values cross-checked against hand-calculated totals and UI quotation `QT-1-00002`

### Known Limitations

- Only two unit tests — no integration, controller, or UI tests
- `QuotationService` validation rejection paths not unit tested
- No automated regression test for DbContext concurrency or DI registration gaps fixed in PR #6

---

## Post-merge documentation (not separate PRs)

After PR #7, lifecycle documentation was added on `main`: `README.md`, `data-model.md`, `api-contract.md`, `design-notes.md`, `ui-flow.md`, `acceptance-criteria.md`, `requirements-analysis.md`, `implementation-plan.md`, `test-strategy.md`, `code-review-notes.md`, and `review-fixes.md`.

---

## Related Documentation

- [implementation-plan.md](implementation-plan.md) — milestones and build sequence
- [acceptance-criteria.md](acceptance-criteria.md) — Core checklist with evidence
- [review-fixes.md](review-fixes.md) — per-PR fix commit references
- [ai-prompts/planning.md](ai-prompts/planning.md) — full prompt acceptance log
