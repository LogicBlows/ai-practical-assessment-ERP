# SmeErp Code Review Notes

Notes on how Cursor-generated code was reviewed during the SmeErp build (Option 3 — .NET Full-Stack SME ERP), based on `debugging-notes.md` (Issues 1–6), `ai-prompts/planning.md`, and the Git commit history.

---

## AI-Assisted Review Summary

After each Cursor prompt, generated code was **not committed blindly**. The review process used three checks before accepting a feature as complete:

### (a) Architectural compliance

Every diff was checked against the constraints established in Prompt #1:

- **No `DbContext` in controllers** — controllers inject Application-layer service interfaces only.
- **`ServiceResult<T>` pattern** — expected failures return failure results, not thrown exceptions for validation.
- **No business logic in Razor views** — views display data and forms; calculations and validation live in services.
- **Tenant scoping** — services accept explicit `companyId`; queries filter by `CompanyId`.

New services were also checked against `Program.cs` DI registrations (a gap that caused Issue 5).

### (b) Correctness against manually verified data

Features were validated against real data, not just a successful build:

- Log in as **both** seeded users and confirm different, correct counts and lists.
- **Hand-calculate** quotation line and header totals and compare to the detail view (Prompt #9).
- **Change Settings** (address, `PrimaryColor`) and confirm the **next** PDF download reflects the change (Prompt #10).
- Run **`dotnet test`** after Prompt #14 and confirm 2/2 mandatory tests pass.

### (c) Completeness — verify the database, not just the response

Several prompts were accepted by Cursor with incomplete deliverables. The review process caught these by **inspecting SQL Server directly in SSMS** (or querying tables) rather than trusting a "success" message or green build:

- Prompt #4 seeded only `Company` rows when Product, Customer, and CompanySetting seed data were also requested → caught via SSMS, fixed in Prompt #5.
- Prompt #10 implemented `IQuotationPdfService` but omitted DI registration → caught when navigating to Quotations after Prompt #12.
- Prompt #12 used `Task.WhenAll` on a shared `DbContext` → caught at runtime when search threw `InvalidOperationException`.

**Operational hygiene** (Issues 1–2): stale background `dotnet run` processes from Cursor verification runs were also caught during review when ports or DLLs were locked — resolved by killing orphaned processes before rebuild.

---

## My Review Observations

Specific observations made during review of Cursor-generated code:

| # | Observation | Impact |
|---|-------------|--------|
| 1 | **Incomplete seed data (Prompt #4)** — Cursor seeded only `Company` records when four entity types were requested. `grep` showed only `CompanyConfiguration.cs` had `HasData()`. | Products, customers, and settings tables were empty until Prompt #5 follow-up. Caught by **SSMS inspection**, not by build output. |
| 2 | **`Task.WhenAll` on shared `DbContext` (SearchService)** — Global search used three parallel EF queries on one scoped `AppDbContext`. | Search page crashed with `InvalidOperationException` on every submit (`debugging-notes.md` Issue 4). |
| 3 | **Same anti-pattern repeated (DashboardService)** — Dashboard KPIs used `Task.WhenAll` with four parallel `CountAsync` calls on the same context. | Dashboard failed to load after Prompt #13 (`debugging-notes.md` Issue 6). Same mistake as Issue 4 in a different service. |
| 4 | **Missing DI registration (`IQuotationPdfService`)** — Service and interface implemented in Prompt #10 but never registered in `Program.cs`. | Entire `QuotationsController` failed to activate; quotations list, detail, and PDF download all broken until fixed (`debugging-notes.md` Issue 5). |
| 5 | **QuestPDF default font ligature corruption** — PDF used default text style without explicit `FontFamily`; "ti" sequences corrupted ("Quotation" → "Quotaon"). | Silent visual defect — data was correct in the UI but wrong in generated PDFs (`debugging-notes.md` Issue 3). |
| 6 | **Stale `dotnet` processes from agent verification** — Background `dotnet run` left running after Cursor "verified" a feature. | Port conflicts and DLL lock errors blocked subsequent builds/migrations (Issues 1–2). Environmental, not a code defect, but recurring during review. |

**Positive patterns observed:** Architectural layer separation was generally followed from Prompt #1 onward. `QuotationTotalsCalculator` extraction for testability (Prompt #14) was a sound refactor. Cross-tenant isolation was consistently implemented in service queries once `ICurrentCompanyService` was in place.

---

## Changes Made After Review

The following fixes were applied after human review caught problems in Cursor output:

| Fix | Files / area |
|-----|----------------|
| **Added missing seed data** for `Product`, `Customer`, and `CompanySetting` | `ProductConfiguration.cs`, `CustomerConfiguration.cs`, `CompanySettingConfiguration.cs`; migration `SeedProductsCustomersSettings` |
| **Replaced `Task.WhenAll` with sequential `await`** in `SearchService` | `src/SmeErp.Infrastructure/Services/SearchService.cs` |
| **Replaced `Task.WhenAll` with sequential `await`** in `DashboardService` | `src/SmeErp.Infrastructure/Services/DashboardService.cs` |
| **Added missing DI registration** | `Program.cs`: `AddScoped<IQuotationPdfService, QuotationPdfService>()` |
| **Fixed PDF font rendering** | `QuotationPdfService.cs`: explicit `FontFamily("Arial")` and `DisableFontFeature(FontFeatures.StandardLigatures)` on `DefaultTextStyle` |

See [review-fixes.md](review-fixes.md) for commit references and verification steps per fix.

---

## Suggestions Rejected (and why)

| Suggestion / direction | Decision | Reason |
|---------------------|----------|--------|
| **Separate Web API project** alongside MVC | Rejected | Core requirements are met by Razor MVC controllers. A parallel REST API would duplicate endpoints and add scope without a Core acceptance criterion requiring it. JWT signing key infrastructure was built for future use but not wired to a separate API layer. |
| **Public self-service tenant registration** | Rejected | Exceeds Core scope. Users and companies are seeded by the developer (`IdentitySeeder`, migration seed data). Prompt #6 explicitly scoped "no registration page." Implementing onboarding would displace time needed for quotations, PDF, search, dashboard, mandatory tests, and lifecycle documentation within the 8–12 hour budget. |
| **Keeping parallel `Task.WhenAll` for performance** | Rejected (after bugs) | Theoretical performance gain is negligible for small SME datasets; EF Core `DbContext` is not safe for concurrent operations on one instance. Sequential awaits adopted in both affected services. |
| **Relying on QuestPDF default font** | Rejected (after visual defect) | Default font fallback caused ligature corruption in production PDFs. Explicit Arial + disabled standard ligatures required for reliable output. |

---

## Related Documentation

- [review-fixes.md](review-fixes.md) — per-fix commit and verification detail
- [debugging-notes.md](debugging-notes.md) — full investigation notes (Issues 1–6)
- [test-strategy.md](test-strategy.md) — what was automated vs manually verified
- [implementation-plan.md](implementation-plan.md) — build sequence and risks
