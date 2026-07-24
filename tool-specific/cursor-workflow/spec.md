# SmeErp Functional Spec (Option 3 — .NET Full-Stack SME ERP)

Summary of the functional specification this project was built against: **Core requirements** (mandatory for assessment completion) plus **Stretch requirements adopted** during development. Based on `ai-prompts/planning.md`, `acceptance-criteria.md`, and the implemented codebase.

---

## Project Option

**Option 3 — .NET Full-Stack SME ERP (Inventory Management)**

A multi-tenant web application for small trading and distribution businesses. Stack: .NET 6, ASP.NET Core MVC, EF Core 6, SQL Server, ASP.NET Identity, QuestPDF. Architecture: Clean Architecture with Razor MVC (not a separate REST API).

**Demo tenants:** Sharma Trading Co. (hardware/electricals, Jaipur) and Verma Distributors (stationery, Pune).

---

## Core Requirements

These features were required for Core scope and are **implemented**.

### 1. Authentication

| Requirement | Implementation |
|-------------|----------------|
| Email/password login | `AccountController` + ASP.NET Identity cookie auth |
| Logout | POST `/Account/Logout` |
| Protected pages | `[Authorize]` on business controllers; redirect to `/Account/Login` |
| Seeded demo users | `admin@sharmatrading.com`, `admin@vermadist.com` (local/demo password) |
| No self-registration | Users seeded at startup only |

### 2. Products — list and search

| Requirement | Implementation |
|-------------|----------------|
| Company-scoped product list | `ProductsController` → `IProductService.SearchAsync` |
| Keyword search | Name, SKU, or barcode (GET `?search=`) |
| **Out of Core scope** | No create/edit/delete UI for products |

### 3. Customers — list and search

| Requirement | Implementation |
|-------------|----------------|
| Company-scoped customer list | `CustomersController` → `ICustomerService.SearchAsync` |
| Keyword search | Name or code (GET `?search=`) |
| **Out of Core scope** | No create/edit/delete UI for customers |

### 4. Quotation creation

| Requirement | Implementation |
|-------------|----------------|
| Create quotation with multiple line items | `Quotations/Create` form with repeatable rows |
| Select customer, set dates, optional notes | `CreateQuotationViewModel` |
| Per-line calculation | Quantity × price, discount %, GST from product, tax, line total |
| Quotation-level totals | Subtotal, discount, tax, grand total |
| Auto quotation number | `QT-{CompanyId}-{sequential}` |
| Backend validation | Customer/product must belong to company; quantity > 0; at least one line |
| **Out of Core scope** | No edit/delete after creation |

### 5. Quotation list and detail

| Requirement | Implementation |
|-------------|----------------|
| List company quotations (newest first) | `QuotationsController.Index` |
| Read-only detail view | `QuotationsController.Details` — header, lines, totals |

### 6. Quotation PDF generation

| Requirement | Implementation |
|-------------|----------------|
| Download PDF from detail page | `QuotationsController.DownloadPdf` → QuestPDF |
| Branded output | Company name, address, GSTIN, PAN, contact, line items, totals |
| Settings-driven content | `PrimaryColor` accent, `InvoiceTerms` footer — from `ICompanySettingsService` |
| No PDF caching | Fresh settings + quotation loaded on every download |
| Settings change reflected | Verified: next PDF after settings update shows new values |

### 7. Company settings

| Requirement | Implementation |
|-------------|----------------|
| Edit company profile | Name, address, city, state, country, PIN, GST, PAN, mobile, email, website |
| Branding settings | `PrimaryColor` (color picker), `InvoiceTerms` |
| Persist to database | `ICompanySettingsService.UpdateAsync` upserts `CompanySetting` rows |
| Defaults when missing | `PrimaryColor` `#1F2937`, generic invoice terms sentence |

### 8. Global search

| Requirement | Implementation |
|-------------|----------------|
| Search from navbar | GET `/Search?keyword=` |
| Minimum 2 characters | `SearchService` returns empty + `KeywordTooShort` flag |
| Search products | Name, SKU, barcode |
| Search customers | Name, code |
| Search quotations | Quotation number, linked customer name |
| Grouped results with links | Products/Customers → filtered list; Quotations → detail page |
| **Out of Core scope** | No autocomplete/AJAX |

### 9. Minimal dashboard

| Requirement | Implementation |
|-------------|----------------|
| Four KPI cards | Total Products, Total Customers, Quotations Today, Pending Quotations |
| Real database counts | `IDashboardService.GetSummaryAsync` — scoped by `CompanyId` |
| Pending definition | Quotations with `ValidUntil >= today` |
| **Out of Core scope** | No charts |

### 10. Mandatory testing

| Requirement | Implementation |
|-------------|----------------|
| Quotation calculation unit test | `QuotationCalculationTests` — `QuotationTotalsCalculator` |
| Settings defaults unit test | `CompanySettingsDefaultsTests` — InMemory EF |
| **Result** | `dotnet test` — 2/2 passing |

### Core non-functional requirements

| Requirement | Status |
|-------------|--------|
| Data persists after restart | SQL Server + EF migrations |
| No secrets in repository | Runtime signing keys; demo password marked local-only |
| Backend validation on quotations | `QuotationService` + `ServiceResult<T>` |
| UI validation/error states | `ModelState`, validation summaries, `404`/`Challenge` |
| README setup works on clean machine | Manually verified |

---

## Stretch Requirements Adopted

These Stretch-tier items were **included** in the build beyond strict Core minimum. Status reflects what is actually in the codebase.

### Multi-tenant company isolation

**Status: Fully implemented**

| Aspect | Detail |
|--------|--------|
| Model | Shared database; `CompanyId` on tenant-scoped entities |
| Tenant root | `Company` (no `CompanyId`) |
| Resolution | `ICurrentCompanyService` → logged-in user's `ApplicationUser.CompanyId` |
| Enforcement | Every service filters by `companyId`; cross-tenant returns failure/`404` |
| Verification | Both seeded users see isolated data (products, customers, quotations, search, dashboard, settings) |

This was architectural from Prompt #1 and is central to the entire application.

### DB-stored JWT signing key

**Status: Infrastructure implemented; not wired to auth**

| Aspect | Detail |
|--------|--------|
| Entity | `SigningKey` (global, not per-tenant) |
| Service | `ISigningKeyService` — `GetActiveKeyAsync`, `RotateKeyAsync` |
| Generation | `RandomNumberGenerator`; 30-day expiry; no hardcoded secrets |
| Startup | `SigningKeySeeder` ensures active key exists |
| **Not implemented** | JWT token generation/validation; cookie auth used instead |

Built in Prompt #7 as Stretch infrastructure for a potential future API layer.

### Role-based access with Proprietor role

**Status: Partially implemented**

| Aspect | Detail |
|--------|--------|
| Roles seeded | `Admin`, `Proprietor` via `IdentitySeeder` |
| Demo users | Both assigned `Proprietor` |
| Authentication | `[Authorize]` on business controllers |
| **Not implemented** | `[Authorize(Roles = "...")]` — no role-based restrictions on actions |
| **Not implemented** | `Admin` role assigned to any user; `CompetencyHead` mentioned in Prompt #1 but not seeded |

Identity and roles exist; authorization is binary (signed in vs not) for Core scope.

### UI theming

**Status: Fully implemented**

| Aspect | Detail |
|--------|--------|
| Theme | Bootswatch Flatly (Bootstrap 5.1.3) via CDN |
| Company accent color | `PrimaryColor` from settings → navbar via `CompanyBrandingViewComponent` |
| Layout | Card-based pages (Dashboard, Settings, Quotation Create/Details) |
| Tables | Striped/hover on list pages |
| PDF branding | Same `PrimaryColor` drives PDF accent color |
| Scope | Visual-only pass (Prompt #11) — no controller/route changes |

---

## Stretch Requirements Not Adopted

For clarity, common Stretch items **not** built in this project:

| Stretch item | Notes |
|--------------|-------|
| Separate REST/Web API project | MVC controllers only |
| Integration / UI automated tests | 2 unit tests only |
| Quotation edit/delete | Create + view only |
| Product/customer CRUD UI | List + search only |
| Self-service tenant registration | Seeded companies/users |
| Charts on dashboard | Four KPI cards only |
| JWT-based API authentication | Signing key table exists; not connected |
| Role-enforced authorization | Roles seeded; not checked on controllers |

---

## Core vs Stretch Summary

```
Core (required)                    Stretch (adopted)
─────────────────────────────────  ─────────────────────────────────
✓ Authentication                   ✓ Multi-tenant isolation
✓ Products list + search           ✓ DB-stored signing key (infra only)
✓ Customers list + search          ✓ Roles + Proprietor (seeded, not enforced)
✓ Quotation create/list/detail     ✓ UI theming (Bootswatch + PrimaryColor)
✓ PDF generation
✓ Company settings
✓ Global search
✓ Dashboard KPIs
✓ 2 mandatory xUnit tests
```

---

## Related Documentation

- [requirements-analysis.md](../../requirements-analysis.md) — detailed functional/non-functional requirements
- [acceptance-criteria.md](../../acceptance-criteria.md) — Core checklist with evidence
- [project-context.md](project-context.md) — how Cursor context was established (Prompt #1)
- [ai-prompts/planning.md](../../ai-prompts/planning.md) — full build prompt log (Prompts #1–#14)
- [pr-description.md](../../pr-description.md) — seven merged feature PRs
