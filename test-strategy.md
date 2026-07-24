# SmeErp Test Strategy

Based on the actual test project (`tests/SmeErp.Application.Tests`), `test-results.md`, and manual verification documented in `debugging-notes.md` and `ai-prompts/planning.md`.

---

## Test Scope

### What is covered by automated testing

| Area | Coverage |
|------|----------|
| **Quotation calculation correctness** | Unit test against `QuotationTotalsCalculator` with known inputs and expected totals |
| **Settings defaults behavior** | Unit test against `CompanySettingsService.GetAsync` when a company has no `CompanySetting` rows |

**Current automated test count:** 2 xUnit tests — both passing (`dotnet test`: 2/2, see `test-results.md`).

### What is NOT covered

| Area | Status |
|------|--------|
| Integration / end-to-end tests | Not implemented |
| UI / browser tests | Not implemented |
| Load / performance tests | Not implemented |
| Controller tests | Not implemented |
| PDF output verification | Not implemented |
| Authentication flow | Not automated |
| Cross-tenant isolation | Not automated |
| Search / dashboard service behavior | Not automated |

This reflects the **Core scope** mandatory test tier (two unit tests) plus manual verification for everything else. Integration and UI testing are **Stretch** tier in the assessment requirements, not required for Core.

---

## Unit Tests

Two mandatory unit tests are implemented in `SmeErp.Application.Tests`.

### 1. Quotation calculation test

| | |
|---|---|
| **Class** | `QuotationCalculationTests` |
| **Method** | `Calculate_WithKnownLineItem_ProducesExpectedTotals` |
| **Target** | `QuotationTotalsCalculator.Calculate` in `SmeErp.Application` |
| **Isolation** | Pure calculation — **no EF Core, no database, no HTTP**. Logic was extracted from `QuotationService` specifically so it could be tested without a live SQL Server connection. |
| **Inputs** | One line: `Quantity = 3`, `UnitPrice = 62.00`, `DiscountPercent = 6%`, `GstPercent = 18%` |
| **Assertions** | `SubTotal = 186.00`, `DiscountAmount = 11.16`, `TaxAmount = 31.47`, `TotalAmount = 206.31` |
| **Why this test** | Quotation totals are business-critical; incorrect math would produce wrong invoices. Figures were independently hand-calculated and cross-checked against a real quotation (`QT-1-00002`) created in the UI during Prompt #9 manual testing. |

### 2. Settings defaults test

| | |
|---|---|
| **Class** | `CompanySettingsDefaultsTests` |
| **Method** | `GetAsync_WhenCompanyHasNoSettings_ReturnsDocumentedDefaults` |
| **Target** | `CompanySettingsService.GetAsync` in `SmeErp.Infrastructure` |
| **Isolation** | Uses **EF Core InMemory** provider with a fresh `AppDbContext` per test run (unique database name via `Guid.NewGuid()`). No SQL Server required. Company record exists but has **no** `CompanySetting` rows. |
| **Assertions** | `PrimaryColor = "#1F2937"` and the documented default `InvoiceTerms` sentence; `result.Succeeded` is true |
| **Why this test** | New or edge-case companies may have no settings rows yet; the service must return sensible defaults rather than throwing or returning null. |

### Test project dependencies

- `xUnit` 2.4.1, `Microsoft.NET.Test.Sdk` 17.1.0
- `Microsoft.EntityFrameworkCore.InMemory` 6.0.36 (settings defaults test only)
- Project references: `SmeErp.Application`, `SmeErp.Infrastructure`

---

## Component Tests

**No component-level tests exist** in this project.

There are no tests for:

- MVC controllers (`AccountController`, `QuotationsController`, etc.)
- Razor views or view components (`CompanyBrandingViewComponent`)
- Model binding or `ModelState` validation behavior

Core scope did not require controller or view tests. Controllers are thin orchestrators; business rules live in Application/Infrastructure services. Manual browser testing was used to verify controller flows (documented in `ai-prompts/planning.md` and `ui-flow.md`).

---

## API / Integration Tests

**No integration tests exist** in this project.

There is no:

- `WebApplicationFactory` setup
- In-memory or test-container SQL Server integration suite
- HTTP-level tests against MVC endpoints
- End-to-end flow test (login → create quotation → download PDF)

Integration testing is listed as **Stretch** in the assessment requirements, not mandatory for Core. The two unit tests satisfy the mandatory test tier.

If integration tests were added in a Stretch phase, high-value candidates would be: `QuotationService.CreateAsync` validation rejection paths, cross-tenant `GetDetailAsync`, and the full quotation create → PDF download pipeline.

---

## Edge Case Tests

Edge cases **considered during development** but **not covered by automated tests**:

| Edge case | Behavior in code | How verified |
|-----------|------------------|--------------|
| Quotation with zero-quantity line items | `QuotationService` returns `ServiceResult.Failure` if any line `Quantity <= 0` | Manual — not automated |
| Quotation with no line items | Controller and service both reject empty line lists | Manual — not automated |
| Customer from another company | `QuotationService` checks `CustomerId` belongs to `companyId` | Manual — not automated |
| Product from another company | `QuotationService` validates all `ProductId` values against `companyId` | Manual — not automated |
| Quotation detail for another tenant's ID | `GetDetailAsync` returns failure; controller returns `404` | Manual — not automated |
| Company with no settings rows | Defaults returned by `GetAsync` | **Automated** — `CompanySettingsDefaultsTests` |
| Search keyword under 2 characters | `SearchService` returns empty results with `KeywordTooShort` | Manual — not automated |
| DbContext concurrency (`Task.WhenAll`) | Fixed in `SearchService` and `DashboardService` (sequential awaits) | Manual — reproduced and fixed per `debugging-notes.md` Issues 4 and 6 |
| Cross-tenant data access (all features) | Every service filters by `CompanyId` | Manual — verified with both seeded users (Sharma vs Verma) via UI and SSMS; not automated |
| Duplicate signing key on startup | `GetActiveKeyAsync` returns existing active key if present | Manual — verified in SSMS after Prompt #7 |
| PDF font ligature corruption | Fixed with explicit Arial font in `QuotationPdfService` | Manual — visual inspection of PDF output (`debugging-notes.md` Issue 3) |
| Missing DI registration (`IQuotationPdfService`) | Quotations controller failed entirely until registered | Manual — caught during search verification (`debugging-notes.md` Issue 5) |

---

## Tests Not Covered (and why)

The following were **verified through manual testing** during development (documented in `ai-prompts/planning.md` Prompts #6–#13 and `debugging-notes.md` Issues 1–6) but are **not covered by automated tests**:

| Area | Manual verification performed | Why not automated |
|------|------------------------------|-----------------|
| **Authentication flow** | Login/logout as both seeded users; correct redirect to Dashboard | Core mandatory tier requires only two unit tests; auth is standard ASP.NET Identity |
| **Role-based authorization** | Roles seeded (`Admin`, `Proprietor`); controllers use `[Authorize]` without role checks | No role restrictions implemented in Core — nothing distinct to test |
| **Cross-tenant isolation** | Sharma user sees only Sharma data; Verma user sees only Verma data; verified across products, customers, quotations, search, dashboard | Would require integration tests with multiple user contexts; Stretch tier |
| **PDF generation correctness** | Download PDF; verify company address, GSTIN, terms, accent color from settings; font fix validated | PDF binary output hard to assert in unit tests without snapshot tooling; Stretch |
| **Settings-to-PDF consistency** | Changed address and `PrimaryColor` in Settings; confirmed next PDF reflected changes | End-to-end flow; Stretch integration test |
| **Search service behavior** | Search for "Havells"; grouped results; keyword-too-short message | No service-level unit tests written |
| **Dashboard KPI counts** | Both users show correct, different counts | Concurrency bug found and fixed manually; no automated regression test |
| **Quotation create validation rejections** | Rules exist in `QuotationService` but happy path only tested in UI | Would need service tests with mocked/in-memory DbContext; deferred |
| **Global search / dashboard concurrency** | `Task.WhenAll` bugs found and fixed twice | Pattern fix verified manually; no test prevents recurrence |

### Rationale

The assessment **Core scope** targets an **8–12 hour time budget**. The **mandatory test tier** (two xUnit tests) is satisfied:

1. Quotation calculation — pure logic, high business value
2. Settings defaults — edge-case service behavior with InMemory EF

Remaining time was allocated to **feature delivery** (quotations, PDF, search, dashboard) and **lifecycle documentation** (README, data-model, api-contract, design-notes, ui-flow, acceptance-criteria, requirements-analysis, implementation-plan) as prioritized by the assessment guidelines.

Automated test gaps are acknowledged honestly in `acceptance-criteria.md`. Stretch-tier work would add integration tests, validation-rejection service tests, and optionally PDF snapshot tests.

---

## Running Tests

From the repository root:

```bash
dotnet test
```

Expected result: **2 passed, 0 failed**.

Latest recorded run: `test-results.md` (2026-07-21, 2/2 passing, 3.6s duration). Re-run after any change to `QuotationTotalsCalculator` or `CompanySettingsService`.

---

## Related Documentation

- [test-results.md](test-results.md) — latest pass/fail results
- [acceptance-criteria.md](acceptance-criteria.md) — Core checklist including mandatory tests item
- [debugging-notes.md](debugging-notes.md) — manual verification and bugs found during development
- [ai-prompts/planning.md](ai-prompts/planning.md) — Prompt #14 mandatory tests entry
