# SmeErp Code Review Findings

Summaries of code review moments documented in `code-review-notes.md` and
`debugging-notes.md`. Framed by finding rather than by prompt number.
See `review-fixes.md` for commit references and verification steps.

---

## Finding: Incomplete seed data delivered (Prompt #4)

**Date discovered:** 2026-07-19  
**Related prompt/feature:** Prompt #4 — Seed Data  
**Related debugging note:** `ai-prompts/planning.md` Prompt #4 response; fixed in Prompt #5

**What was found:**  
Cursor seeded only `Company` records via `HasData()` when the prompt
requested seed data for `Company`, `CompanySetting`, `Product`, and
`Customer`. Build succeeded and migration applied without error, but
SSMS inspection showed `Products`, `Customers`, and `CompanySettings`
tables were empty. Confirmed via `grep` that only `CompanyConfiguration.cs`
contained `HasData()`.

**Review action:**  
Did not accept Prompt #4 as complete. Issued follow-up Prompt #5 to add
missing seed configurations and migration `SeedProductsCustomersSettings`.

**Fix applied:**  
Commit `226516f` — "Add missing seed data for products, customers, settings (prompt #5)"

**Verification:**  
Re-inspected all four tables in SSMS; 2 settings, 4 products, and 3
customers per company with correct business-type data.

**Lesson:**  
Verify database state directly after seed migrations — do not trust build
success or AI "done" messages alone.

---

## Finding: DbContext concurrency in SearchService (Prompt #12)

**Date discovered:** 2026-07-20  
**Related prompt/feature:** Prompt #12 — Global Search  
**Related debugging note:** `debugging-notes.md` Issue 4

**What was found:**  
`SearchService.SearchAsync` used `Task.WhenAll` to run three EF Core
queries in parallel (`SearchProductsAsync`, `SearchCustomersAsync`,
`SearchQuotationsAsync`) against a single scoped `AppDbContext`. Every
search submission threw `InvalidOperationException: A second operation
was started on this context instance before a previous operation
completed`.

**Review action:**  
Rejected search feature as working until concurrency fix applied.
Replaced `Task.WhenAll` with sequential `await` calls.

**Fix applied:**  
Commit `7c20d79` — "Add Global Search and Dashboard KPI cards (prompts #12 and #13)"  
File: `src/SmeErp.Infrastructure/Services/SearchService.cs`

**Verification:**  
Search for "Havells" returned results without exception. Cross-tenant
isolation confirmed — Sharma and Verma users see only their own matches.

**Lesson:**  
Never use `Task.WhenAll` (or any concurrent execution) against a single
scoped `DbContext` instance.

---

## Finding: DbContext concurrency in DashboardService (Prompt #13)

**Date discovered:** 2026-07-20  
**Related prompt/feature:** Prompt #13 — Dashboard KPI Cards  
**Related debugging note:** `debugging-notes.md` Issue 6

**What was found:**  
`DashboardService.GetSummaryAsync` used `Task.WhenAll` with four parallel
`CountAsync` queries on the same scoped `AppDbContext`. Dashboard page
failed to render with the same `InvalidOperationException` seen in
SearchService — the identical anti-pattern in a different service.

**Review action:**  
Rejected dashboard feature as working until fix applied. Replaced
`Task.WhenAll` with sequential `await` calls. Scanned codebase for
other `Task.WhenAll` usages against `DbContext` — none remaining.

**Fix applied:**  
Commit `7c20d79` — "Add Global Search and Dashboard KPI cards (prompts #12 and #13)"  
File: `src/SmeErp.Infrastructure/Services/DashboardService.cs`

**Verification:**  
Dashboard loads for both seeded users with correct, different counts:
- Sharma: 4 products, 3 customers, 3 quotations today, 3 pending
- Verma: 4 products, 3 customers, 0 quotations today, 0 pending

**Lesson:**  
Fixing the pattern in one service does not prevent recurrence — review
new EF service code for parallel `DbContext` access explicitly.

---

## Finding: Missing DI registration for IQuotationPdfService (Prompt #10, discovered during Prompt #12)

**Date discovered:** 2026-07-20  
**Related prompt/feature:** Prompt #10 — Company Settings and PDF (implementation); discovered during Prompt #12 verification  
**Related debugging note:** `debugging-notes.md` Issue 5

**What was found:**  
`IQuotationPdfService` / `QuotationPdfService` was implemented in
Prompt #10 and injected into `QuotationsController`, but
`builder.Services.AddScoped<IQuotationPdfService, QuotationPdfService>()`
was never added to `Program.cs`. After Prompt #12, navigating to any
Quotations page failed with `InvalidOperationException: Unable to resolve
service for type 'IQuotationPdfService' while attempting to activate
'QuotationsController'`.

**Review action:**  
Audited all Application-layer service interfaces against `Program.cs`
registrations. Added missing line. Re-tested quotations module and search.

**Fix applied:**  
Commit `7c20d79` — "Add Global Search and Dashboard KPI cards (prompts #12 and #13)"  
File: `src/SmeErp.Web/Program.cs`

**Verification:**  
`dotnet build` succeeded. Quotation Details loads. Download PDF streams
valid file. Global search and cross-tenant isolation re-verified.

**Lesson:**  
When adding a new service interface, verify DI registration in
`Program.cs` and confirm controller pages load before accepting the feature.

---

## Finding: QuestPDF font ligature corruption in generated PDFs (Prompt #10)

**Date discovered:** 2026-07-20  
**Related prompt/feature:** Prompt #10 — Company Settings and Quotation PDF Generation  
**Related debugging note:** `debugging-notes.md` Issue 3

**What was found:**  
Downloaded quotation PDFs displayed corrupted text wherever words
contained "ti" — e.g. "Quotation" rendered as "Quotaon", "Valid until"
as "Valid unl". Browser detail view showed correct data; defect was in
PDF rendering only. Root cause: QuestPDF default font (Lato) with standard
ligatures enabled via implicit `DefaultTextStyle` (font size only, no
explicit `FontFamily`).

**Review action:**  
Compared PDF output against browser detail view. Applied explicit Arial
`FontFamily` and disabled `FontFeatures.StandardLigatures` on page
`DefaultTextStyle` in `QuotationPdfService.cs`.

**Fix applied:**  
Commit `8e2f3b5` — "Add Company Settings page and Quotation PDF generation with font fix (prompt #10)"  
File: `src/SmeErp.Infrastructure/Services/QuotationPdfService.cs`

**Verification:**  
Regenerated PDF — "Quotation" and "until" render correctly. PDF binary
shows Arial references, no Lato, no corrupted strings. Settings-to-PDF
consistency re-confirmed after font fix (address and `PrimaryColor`
changes reflected on next download).

**Lesson:**  
Do not rely on QuestPDF default font fallback for production PDF output;
set explicit font family and ligature settings and visually inspect generated files.

---

## Related Documentation

- [code-review-notes.md](../code-review-notes.md) — full review process and rejected suggestions
- [review-fixes.md](../review-fixes.md) — per-fix commit and verification table
- [debugging-notes.md](../debugging-notes.md) — detailed investigation narratives (Issues 1–6)
- [ai-prompts/planning.md](planning.md) — complete prompt log (unchanged)
