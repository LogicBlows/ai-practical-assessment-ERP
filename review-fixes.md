# SmeErp Review Fixes

Per-fix documentation for changes applied after human review of Cursor-generated code. Each entry covers what was found, where it was fixed (commit), and how it was verified.

---

## Fix 1 — Missing seed data (Product, Customer, CompanySetting)

### What was found

Prompt #4 requested `HasData()` seeding for `Company`, `CompanySetting`, `Product`, and `Customer`. Cursor only seeded **Company** records (`CompanyConfiguration.cs` / migration `SeedCompanies`). Inspecting the database in SSMS showed `Products`, `Customers`, and `CompanySettings` tables were empty.

### Where it was fixed

| Item | Detail |
|------|--------|
| **Commit** | `226516f` — **"Add missing seed data for products, customers, settings (prompt #5)"** |
| **Follow-up prompt** | Prompt #5 in `ai-prompts/planning.md` |
| **Files** | `CompanySettingConfiguration.cs`, `ProductConfiguration.cs`, `CustomerConfiguration.cs` (seed data classes under `Persistence/Seed/`) |
| **Migration** | `20260719185511_SeedProductsCustomersSettings` |

### How it was verified

- Ran `dotnet ef database update` and re-inspected all four tables in SSMS.
- Confirmed 2 settings, 4 products, and 3 customers per company with realistic Indian business data.
- Subsequent manual testing (Prompt #8) confirmed Sharma Trading sees 4 hardware products and 3 Jaipur customers; Verma Distributors sees 4 stationery products and 3 Pune customers.

---

## Fix 2 — DbContext concurrency in SearchService (`Task.WhenAll`)

### What was found

After Prompt #12 (global search), submitting a search threw:

`InvalidOperationException: A second operation was started on this context instance before a previous operation completed.`

`SearchService.SearchAsync` used `Task.WhenAll` to run `SearchProductsAsync`, `SearchCustomersAsync`, and `SearchQuotationsAsync` in parallel — all on the same scoped `AppDbContext`.

### Where it was fixed

| Item | Detail |
|------|--------|
| **Commit** | `7c20d79` — **"Add Global Search and Dashboard KPI cards (prompts #12 and #13)"** |
| **File** | `src/SmeErp.Infrastructure/Services/SearchService.cs` |
| **Change** | Replaced `Task.WhenAll` with three sequential `await` calls |

### How it was verified

- Rebuilt `SmeErp.Infrastructure` and submitted a search from the navbar (e.g. keyword `"Havells"`) — results returned with no exception.
- Confirmed grouped results (Products, Customers, Quotations) render on `/Search`.
- **Cross-tenant isolation:** Sharma's user sees only Sharma matches; Verma's user sees only Verma matches (`debugging-notes.md` Issue 4).

---

## Fix 3 — DbContext concurrency in DashboardService (`Task.WhenAll`)

### What was found

After Prompt #13 (dashboard KPI cards), loading `/Dashboard` threw the same `InvalidOperationException`. `DashboardService.GetSummaryAsync` used `Task.WhenAll` with four parallel `CountAsync` queries on one `AppDbContext` — the identical anti-pattern as SearchService.

### Where it was fixed

| Item | Detail |
|------|--------|
| **Commit** | `7c20d79` — **"Add Global Search and Dashboard KPI cards (prompts #12 and #13)"** (same commit as search fix) |
| **File** | `src/SmeErp.Infrastructure/Services/DashboardService.cs` |
| **Change** | Replaced `Task.WhenAll` with four sequential `await CountAsync` calls |

### How it was verified

- Dashboard page loads for both seeded users without exception.
- **Sharma Trading** (`admin@sharmatrading.com`): 4 products, 3 customers, 3 quotations today, 3 pending.
- **Verma Distributors** (`admin@vermadist.com`): 4 products, 3 customers, 0 quotations today, 0 pending — confirming correct, different per-tenant counts (`debugging-notes.md` Issue 6).
- Codebase scan confirmed no remaining `Task.WhenAll` against `DbContext`.

---

## Fix 4 — Missing DI registration for `IQuotationPdfService`

### What was found

After implementing global search, navigating to any Quotations page failed:

`InvalidOperationException: Unable to resolve service for type 'IQuotationPdfService' while attempting to activate 'QuotationsController'.`

`QuotationPdfService` was implemented in Prompt #10 but `builder.Services.AddScoped<IQuotationPdfService, QuotationPdfService>()` was never added to `Program.cs`. Other services (`ICompanySettingsService`, `ISearchService`, `IDashboardService`) were registered correctly.

### Where it was fixed

| Item | Detail |
|------|--------|
| **Commit** | `7c20d79` — **"Add Global Search and Dashboard KPI cards (prompts #12 and #13)"** (discovered and fixed during search/PDF verification) |
| **File** | `src/SmeErp.Web/Program.cs` |
| **Change** | Added `builder.Services.AddScoped<IQuotationPdfService, QuotationPdfService>();` |

### How it was verified

- `dotnet build SmeErp.sln` succeeded.
- Quotation Details page loads for an existing quotation.
- **Download PDF** button streams a valid `application/pdf` file.
- Re-verified global search and cross-tenant isolation after the DI fix (`debugging-notes.md` Issue 5).

---

## Fix 5 — QuestPDF font ligature corruption in generated PDFs

### What was found

Downloaded quotation PDFs showed corrupted text wherever words contained `"ti"` — e.g. `"Quotation"` rendered as `"Quotaon"`, `"Valid until"` as `"Valid unl"`. Browser detail view showed correct text; the defect was in PDF rendering only. Root cause: QuestPDF default font (Lato) with standard ligatures enabled via implicit `DefaultTextStyle`.

### Where it was fixed

| Item | Detail |
|------|--------|
| **Commit** | `8e2f3b5` — **"Add Company Settings page and Quotation PDF generation with font fix (prompt #10)"** |
| **File** | `src/SmeErp.Infrastructure/Services/QuotationPdfService.cs` |
| **Change** | Set `DefaultTextStyle` with `FontFamily("Arial")`, `FontSize(10)`, and `DisableFontFeature(FontFeatures.StandardLigatures)` |

### How it was verified

- Regenerated a quotation PDF — `"Quotation"` and `"until"` render correctly.
- Inspected PDF binary: Arial font references present; no Lato; no corrupted strings.
- **Settings-to-PDF consistency re-confirmed:** changed address and `PrimaryColor` in Settings; next PDF download reflected updated values (`debugging-notes.md` Issue 3).

---

## Environmental issues (review process, not code fixes)

These were caught during review but resolved operationally rather than by code changes:

| Issue | Problem | Resolution | Verified by |
|-------|---------|------------|-------------|
| **Issue 1** — Port in use | Stale `dotnet run` blocking ports 7211/5057 | Kill orphaned `dotnet` process | App starts; login works for both users |
| **Issue 2** — DLL locked | Stale process blocked `SmeErp.Infrastructure.dll` during build/migration | Kill process before rebuild | `dotnet build` and `dotnet ef database update` succeed; `SigningKeys` table populated in SSMS |

Documented in `debugging-notes.md` Issues 1–2. Review takeaway: stop background verification runs before the next build or migration.

---

## Summary table

| Fix | Commit | Primary file(s) | Verification method |
|-----|--------|-----------------|---------------------|
| Missing seed data | `226516f` | Seed configurations + migration | SSMS table inspection; Prompt #8 manual tenant test |
| SearchService concurrency | `7c20d79` | `SearchService.cs` | Search submit; cross-tenant results |
| DashboardService concurrency | `7c20d79` | `DashboardService.cs` | Dashboard KPI counts both tenants |
| Missing `IQuotationPdfService` DI | `7c20d79` | `Program.cs` | Quotations pages + PDF download |
| PDF font corruption | `8e2f3b5` | `QuotationPdfService.cs` | Visual PDF check; settings change → next PDF |

---

## Related Documentation

- [code-review-notes.md](code-review-notes.md) — review process and rejected suggestions
- [debugging-notes.md](debugging-notes.md) — full investigation narratives
- [ai-prompts/planning.md](ai-prompts/planning.md) — Prompts #5, #10, #12, #13 acceptance notes
