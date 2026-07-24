# SmeErp Implementation Plan

Based on the actual build sequence recorded in `ai-prompts/planning.md` (Prompts #1–#14) and Git commit/PR history.

---

## Overview

SmeErp was built **incrementally**, one feature per scoped Cursor prompt, merged via **separate feature branches and pull requests**. Each step added a vertical slice of functionality on top of a fixed Clean Architecture foundation established in the first prompt.

The approach:

1. **One feature per branch/PR** — auth, products/customers, quotations, settings/PDF, styling, search/dashboard, tests each landed in their own PR before merging to main.
2. **Architectural constraints fixed upfront** — Prompt #1 defined layer boundaries, `ServiceResult<T>`, no business logic in views/controllers, and `CompanyId` multi-tenancy; every subsequent prompt built within those rules.
3. **Manual verification before accepting each feature** — run the app, inspect SQL Server (SSMS), hand-calculate quotation totals, test both seeded tenants, confirm PDF output.
4. **Documentation generated after implementation** — lifecycle docs (README, data-model, api-contract, etc.) drafted via Cursor and reviewed before commit.

No big-bang delivery: the app was runnable (with increasing capability) after Prompt #6 (auth), and each later prompt extended an already-working system.

---

## Task Breakdown

Actual sequence of work completed, in order:

| # | Prompt | Work completed | Git / PR |
|---|--------|----------------|----------|
| 1 | Solution scaffold | .NET 6 Clean Architecture solution: Domain, Application, Infrastructure, Shared, Web, Application.Tests; project references wired | `f107a08` — scaffold |
| 2 | Domain entities | `Company`, `CompanySetting`, `Product`, `Customer`, `Quotation`, `QuotationLine` entity classes | `1b475da` |
| 3 | DbContext / migrations | `AppDbContext`, Fluent API config, `ApplicationUser`, `InitialCreate` migration, SQL Server registration | `8e2935f` |
| 4 | Seed data (companies) | Two demo companies via `HasData()`; `SeedCompanies` migration | `f539c65` |
| 5 | Seed data correction | Products, customers, company settings per company; `SeedProductsCustomersSettings` migration (follow-up after incomplete Prompt #4) | `226516f` |
| 6 | Authentication / roles / current company | ASP.NET Identity, seeded roles (`Admin`, `Proprietor`) and users, login/logout, `ICurrentCompanyService`, `[Authorize]` dashboard placeholder | `e73ccec` → PR #1 `feature/auth-and-roles` |
| 7 | DB-stored JWT signing key | `SigningKey` entity, `ISigningKeyService`, runtime key generation/rotation, `AddSigningKeyTable` migration, startup seeder | `d46e8eb` (same PR #1) |
| 8 | Products / Customers list + search | `IProductService`, `ICustomerService`, controllers, Razor Index views, nav links | `e7b2fab` → PR #2 `feature/products-customers` |
| 9 | Quotation create / list / detail | `IQuotationService`, line/total calculations, create form with repeatable lines, Index and Details views | `d799cf2` → PR #3 `feature/quotations` |
| 10 | Company Settings + PDF | `ICompanySettingsService`, Settings page, QuestPDF integration, PDF download; font ligature bug fixed | `8e2f3b5` → PR #4 `feature/settings-and-pdf` |
| 11 | UI styling pass | Bootswatch Flatly CDN, `CompanyBrandingViewComponent`, card layouts, striped tables | `5425453` → PR #5 `feature/ui-styling` |
| 12 | Global search | `ISearchService`, `SearchController`, navbar search form, grouped results view; DbContext concurrency fix | `7c20d79` (partial) → PR #6 `feature/search-and-dashboard` |
| 13 | Dashboard KPIs | `IDashboardService`, four KPI cards; DbContext concurrency fix (recurring pattern) | `7c20d79` → PR #6 |
| 14 | Mandatory xUnit tests | `QuotationTotalsCalculator` extraction, quotation calculation test, settings defaults test | `767cbdc` → PR #7 `feature/tests` |

**Post-Core documentation** (after PR #7, on main):

| Document | Commit |
|----------|--------|
| `README.md` | `a0b6c69` |
| `data-model.md` | `eab0c15` |
| `api-contract.md` | `93cf6dc` |
| `design-notes.md` | `0359a09` |
| `ui-flow.md` | `9dc1b3a` |
| `acceptance-criteria.md` | `cbc7830` |
| `requirements-analysis.md` | `3609769` |

---

## Milestones

### Milestone 1 — Foundation

**Scope:** Scaffold + entities + database + seed data  
**Prompts:** #1–#5  
**Outcome:** Runnable solution structure, SQL Server schema, two seeded companies with products, customers, and settings.

| Deliverable | Status |
|-------------|--------|
| Clean Architecture project layout | Done |
| Domain entities with `CompanyId` on tenant-scoped types | Done |
| EF Core migrations applied | Done |
| Reference seed data (2 companies, 4 products each, 3 customers each, settings) | Done |

**Verification:** `dotnet ef database update`; inspect tables in SSMS.

---

### Milestone 2 — Auth & Multi-Tenancy

**Scope:** Authentication, roles, current-company resolution, signing key infrastructure  
**Prompts:** #6–#7  
**PR:** #1 `feature/auth-and-roles`

| Deliverable | Status |
|-------------|--------|
| Login / logout flow | Done |
| Seeded users linked to companies | Done |
| `ICurrentCompanyService` | Done |
| DB-stored signing key (not yet wired to JWT auth) | Done |

**Verification:** Log in as both seeded users; confirm correct `CompanyId` on dashboard placeholder.

---

### Milestone 3 — Core Business Features

**Scope:** Products, customers, quotations, settings, PDF  
**Prompts:** #8–#10  
**PRs:** #2 `feature/products-customers`, #3 `feature/quotations`, #4 `feature/settings-and-pdf`

| Deliverable | Status |
|-------------|--------|
| Product and customer list + search (tenant-scoped) | Done |
| Quotation create / list / detail with calculations | Done |
| Company settings (profile + PrimaryColor + InvoiceTerms) | Done |
| Quotation PDF download branded from settings | Done |

**Verification:** Hand-calculate quotation totals vs UI; change settings and confirm next PDF reflects changes; cross-tenant isolation with both users.

---

### Milestone 4 — Polish & Verification

**Scope:** UI styling, search, dashboard, tests, lifecycle documentation  
**Prompts:** #11–#14 + documentation prompts  
**PRs:** #5 `feature/ui-styling`, #6 `feature/search-and-dashboard`, #7 `feature/tests`

| Deliverable | Status |
|-------------|--------|
| Bootswatch theme and card-based UI | Done |
| Global search (products, customers, quotations) | Done |
| Dashboard KPI cards | Done |
| Mandatory xUnit tests (2/2 passing) | Done |
| Lifecycle documentation suite | Done |

**Verification:** `dotnet test`; search and dashboard for both tenants; README setup on clean machine.

---

## AI Usage Plan

The actual prompting approach used throughout the project:

### 1. Fixed constraints in the first prompt

Prompt #1 established non-negotiable rules reused in every later prompt:

- Clean Architecture layer boundaries
- `ServiceResult<T>` for expected failures
- No business logic in Razor views or controllers
- No direct `DbContext` in controllers
- `CompanyId` multi-tenancy on tenant-scoped entities
- EF Core 6 + SQL Server Code-First

### 2. One scoped prompt per feature

Each subsequent prompt targeted a single deliverable (e.g. "build quotation create/list/detail", "add global search"). Prompts included explicit scope boundaries ("do not implement X yet — that is a separate step") to prevent scope creep.

### 3. Manual verification before acceptance

Before logging a prompt as accepted in `planning.md`, the developer:

- Ran `dotnet build` and `dotnet run`
- Tested in the browser (login, create quotation, download PDF, search)
- Queried SQL Server in SSMS for seed data and signing keys
- Hand-calculated quotation totals and compared to UI output
- Confirmed cross-tenant isolation (Sharma vs Verma users see different data)
- Ran `dotnet test` after Prompt #14

Failures were addressed via follow-up prompts (Prompt #5 seed correction) or debugging sessions documented in `debugging-notes.md`.

### 4. Documentation generated via Cursor, reviewed before commit

After implementation prompts, separate meta-prompts appended entries to `ai-prompts/planning.md` and `ai-prompts/documentation.md`. Lifecycle docs (README, data-model, api-contract, etc.) were drafted by Cursor from the codebase and reviewed for accuracy before commit. The "My Understanding" section in `requirements-analysis.md` was explicitly reserved for the developer's own words.

### 5. AI-assisted debugging, human validation

When bugs appeared (font corruption, DbContext concurrency, missing DI), Cursor helped diagnose and propose fixes; the developer validated fixes by re-running the app and checking output before accepting.

---

## Risks

Real risks that materialized or were relevant during development:

| Risk | Impact | What happened | Mitigation |
|------|--------|---------------|------------|
| **DbContext concurrency with `Task.WhenAll`** | Search and dashboard pages threw `InvalidOperationException`; features unusable | Occurred twice: `SearchService` (Issue 4) and `DashboardService` (Issue 6) | Replace parallel `Task.WhenAll` with sequential `await`; never run concurrent operations on one scoped `DbContext` |
| **Missing DI registration** | Entire `QuotationsController` failed to activate; quotations module broken | `IQuotationPdfService` implemented in Prompt #10 but not registered in `Program.cs` until found during search verification (Issue 5) | Audit new service interfaces against `Program.cs` after each feature; verify controller pages load before merging |
| **QuestPDF font rendering** | PDF text corrupted ("Quotation" → "Quotaon"); unprofessional output | Default QuestPDF font ligature handling dropped "ti" sequences (Issue 3) | Set explicit `FontFamily("Arial")` and disable standard ligatures on `DefaultTextStyle` |
| **Incomplete AI deliverables** | Seed data gap — only companies seeded, not products/customers/settings | Prompt #4 accepted incompletely; caught by SSMS inspection | Follow-up Prompt #5; always verify database state, not just build success |
| **Stale `dotnet` processes locking ports/DLLs** | `dotnet run` port conflict; `dotnet build` / `ef database update` failed with file-in-use errors | Background verification runs left orphaned processes (Issues 1 and 2) | Kill stale `dotnet` processes before rebuild or migration |
| **Multi-tenant data leakage** | Cross-company data visible to wrong user | Risk mitigated by design; manually verified after auth, search, and dashboard | Every service method takes explicit `companyId`; filter all queries; test with both seeded users |
| **Hardcoded secrets** | Signing keys or credentials in source control | Assessment requirement to avoid secrets in repo | Runtime key generation via `RandomNumberGenerator`; demo password marked local-only in code comment |
| **Calculation errors in quotations** | Incorrect totals on invoices | Business-critical for ERP trust | Extracted `QuotationTotalsCalculator`; hand-verified against UI; mandatory xUnit test |
| **Settings not reflected in PDF** | Stale branding on downloaded documents | Would break settings-to-PDF acceptance criterion | No PDF caching; load settings fresh on every `DownloadPdf` request; manually verified after settings change |

### Risks not encountered but worth noting for future work

- **No automated integration tests** — regressions (e.g. recurring DbContext pattern) caught manually, not by CI.
- **JWT signing key unused** — infrastructure built but not connected to auth; future API layer needs careful integration.
- **Single user per company seeded** — schema supports more; no user-management UI if multi-user becomes a requirement.

---

## Related Documentation

- [ai-prompts/planning.md](ai-prompts/planning.md) — full prompt log with acceptance notes
- [debugging-notes.md](debugging-notes.md) — Issues 1–6 encountered during build
- [acceptance-criteria.md](acceptance-criteria.md) — Core checklist with evidence
- [requirements-analysis.md](requirements-analysis.md) — functional/non-functional requirements and assumptions
