# Debugging Notes

Session notes for environment and tooling issues encountered while building SmeErp.


## Issue 1 — Port Already In Use When Starting the Web App

### Problem
After Prompt #6 (authentication), running `dotnet run` failed because the
app could not bind to its configured ports (`https://localhost:7211` /
`http://localhost:5057`) — another instance of SmeErp.Web was already
listening from a prior run that had not been stopped.

### How I Investigated
- Read the console output from `dotnet run`; it reported that the address
  was already in use.
- Checked which process was holding the port (Task Manager / `netstat`
  for listeners on 7211 and 5057).
- Confirmed a leftover `dotnet` host process from an earlier dev session.

### How AI Helped
- Cursor had previously started the web app in the background to verify
  Identity seeding and login flow.
- Pointed out that the background `dotnet run` was still running and
  blocking the port on the next launch attempt.

### What I Validated
- Stopped the orphaned `dotnet` process.
- Re-ran `dotnet run` from `src/SmeErp.Web` — app started cleanly.
- Logged in as both seeded users; dashboard showed the correct CompanyId
  for each account.

### Final Fix
Stop any stale `dotnet run` / SmeErp.Web host before starting a new dev
session. On Windows: `Get-Process dotnet` and end the orphaned process,
or close the terminal that launched the prior run.


## Issue 2 — Build Failure Due to Locked DLL from Stale Process

### Problem
While working through Prompt #7 (DB-stored JWT signing key), `dotnet build`
and/or `dotnet ef database update` failed with an MSBuild copy error:
`SmeErp.Infrastructure.dll` could not be written because it was locked by
another process. This blocked generating or applying migration
`20260720054641_AddSigningKeyTable`.

### How I Investigated
- Read the full build output; MSBuild reported access denied / file in use
  when copying to `src\SmeErp.Infrastructure\bin\Debug\net6.0\SmeErp.Infrastructure.dll`
  (MSB3027-style error).
- Listed running `dotnet` processes with `Get-Process dotnet` in PowerShell.
- Matched the locking process to a background `dotnet run --no-build` that
  Cursor had started earlier to verify `SigningKeySeeder` inserted a row on
  first startup (PID 80624 in the agent session).

### How AI Helped
- Cursor identified that the verification step (`dotnet run` in the
  background after implementing `ISigningKeyService` / `SigningKeyService`)
  was still holding `SmeErp.Infrastructure.dll` and `SmeErp.Web.dll` in memory.
- Suggested killing the stale process rather than changing any project code,
  since the failure was environmental rather than a compile error.

### What I Validated
- Ran `Stop-Process -Id 80624 -Force` (or killed the matching `dotnet`
  process via Task Manager).
- Re-ran `dotnet build SmeErp.sln` — succeeded with 0 warnings / 0 errors.
- Re-ran `dotnet ef database update` with startup project `SmeErp.Web` —
  migration `20260720054641_AddSigningKeyTable` applied successfully.
- Queried `SigningKeys` in SSMS: one active row with a base64 `KeyValue`,
  `IsActive = 1`, and `ExpiresAt` approximately 30 days after `CreatedAt`
  (no hardcoded secret in source or config).

### Final Fix
Before rebuilding or running EF migrations after a Cursor/agent verification
run, ensure no background `dotnet run` is still alive:
`Get-Process dotnet | Stop-Process -Force` (target the specific PID if
multiple dotnet processes are running). Then rebuild and apply migrations
as normal.


## Issue 3 — QuestPDF Font Rendering Corrupted 'ti' Character Sequences

### Problem
After Prompt #10 (Quotation PDF generation), downloaded quotation PDFs
displayed corrupted text wherever a word contained the letter sequence
"ti" — for example, "Quotation" rendered as "Quotaon" and "Valid until"
rendered as "Valid unl". The missing characters made the PDF look
unprofessional and could confuse customers reading payment terms or
document titles.

### How I Investigated
- Opened a generated quotation PDF and compared visible text against the
  quotation detail page in the browser — confirmed the source data was
  correct but the PDF output was wrong.
- Noticed the corruption always involved the "ti" pair, suggesting a
  font ligature or glyph-substitution issue rather than bad data.
- Inspected QuotationPdfService.cs and saw the page used only
  `page.DefaultTextStyle(x => x.FontSize(10))` with no explicit
  FontFamily, meaning QuestPDF fell back to its bundled default font
  (Lato) and its ligature settings.
- Researched QuestPDF font/ligature behaviour — standard ligatures merge
  character pairs like "ti" into a single glyph, which can drop characters
  when font subsetting or rendering goes wrong.

### How AI Helped
- Cursor identified the pattern ("ti" sequences only) as a known
  QuestPDF ligature issue with the default font rather than a data bug.
- Suggested fixing it by setting an explicit, reliably available font
  (Arial) on the document's DefaultTextStyle via `.FontFamily()`, and
  optionally disabling `FontFeatures.StandardLigatures`.
- Applied the change in QuotationPdfService.cs and verified the generated
  PDF no longer embedded Lato (Arial references present instead).

### What I Validated
- Rebuilt SmeErp.Infrastructure after stopping the stale SmeErp.Web
  process that was locking the DLL.
- Regenerated a quotation PDF and confirmed "Quotation" and "until" render
  correctly with no missing characters.
- Inspected the PDF binary: Arial font references present, no Lato
  references, and no corrupted strings such as "Quotaon" or "Valid unl".
- Re-confirmed Settings-to-PDF consistency still works after the font
  fix (address and PrimaryColor changes reflected on the next download).

### Final Fix
In `QuotationPdfService.cs`, set an explicit default text style on each
page instead of relying on QuestPDF's automatic font fallback:

```csharp
page.DefaultTextStyle(DefaultPdfTextStyle);

private static TextStyle DefaultPdfTextStyle =>
    TextStyle.Default
        .FontFamily("Arial")
        .FontSize(10)
        .DisableFontFeature(FontFeatures.StandardLigatures);
```

This ensures all header, content, and footer text inherits Arial with
standard ligatures disabled, preventing "ti" (and similar) character
pairs from being merged incorrectly.


## Issue 4 — DbContext Concurrency Exception in Parallel Search Queries

### Problem
After Prompt #12 (global search), submitting a search from the navbar
or Search/Index page threw `InvalidOperationException: A second operation
was started on this context instance before a previous operation
completed`. Search never returned results.

### How I Investigated
- Read the full exception stack trace; it pointed to
  `SearchService.SearchAsync` and EF Core's `DbContext` concurrency
  checks.
- Opened `SearchService.cs` and found `Task.WhenAll` running
  `SearchProductsAsync`, `SearchCustomersAsync`, and
  `SearchQuotationsAsync` at the same time — all three used the same
  scoped `AppDbContext` instance injected into `SearchService`.
- Recognised this as the same EF Core anti-pattern: `DbContext` is not
  safe for concurrent operations on a single scoped instance.

### How AI Helped
- Cursor identified that EF Core's `DbContext` is not thread-safe for
  concurrent operations on a single instance.
- Recommended the same fix used elsewhere: replace `Task.WhenAll` with
  sequential `await` calls, one query at a time.

### What I Validated
- Rebuilt SmeErp.Infrastructure after the change.
- Searched for a known Sharma Trading product (e.g. "Havells") — search
  returned one product with no exception.
- Confirmed cross-tenant isolation: Sharma's user sees only Sharma
  products/customers/quotations in search results; Verma's user sees
  only Verma's data.

### Final Fix
In `SearchService.SearchAsync`, replace parallel task execution with
sequential awaits:

```csharp
var products = await SearchProductsAsync(companyId, trimmedKeyword, cancellationToken);
var customers = await SearchCustomersAsync(companyId, trimmedKeyword, cancellationToken);
var quotations = await SearchQuotationsAsync(companyId, trimmedKeyword, cancellationToken);
```

Do not use `Task.WhenAll` (or any other concurrent execution) against the
same `DbContext` instance.


## Issue 5 — Missing DI Registration for IQuotationPdfService

### Problem
After implementing global search, navigating to any Quotations page
failed with `InvalidOperationException: Unable to resolve service for
type 'IQuotationPdfService' while attempting to activate
'QuotationsController'`. The entire quotations module was unusable,
including quotation details and PDF download.

### How I Investigated
- Read the exception message — ASP.NET Core could not construct
  `QuotationsController` because `IQuotationPdfService` was not
  registered in the DI container.
- Opened `Program.cs` and compared service registrations against
  `QuotationsController`'s constructor dependencies.
- Found `IQuotationPdfService` / `QuotationPdfService` was implemented
  in Prompt #10 but never added to `builder.Services`; other recent
  services (`ICompanySettingsService`, `ISearchService`,
  `IDashboardService`) were registered correctly.

### How AI Helped
- Cursor audited all Application-layer service interfaces against
  `Program.cs` registrations.
- Added the missing line:
  `builder.Services.AddScoped<IQuotationPdfService, QuotationPdfService>();`
- Stopped a stale `SmeErp.Web` process that was locking DLLs, then
  rebuilt to pick up the change.

### What I Validated
- `dotnet build SmeErp.sln` succeeded with 0 warnings.
- Quotation Details page loads for an existing quotation.
- Download PDF button generates and streams a valid PDF file.
- Re-verified global search and cross-tenant isolation after the DI fix.

### Final Fix
Add the missing registration in `Program.cs`:

```csharp
builder.Services.AddScoped<IQuotationPdfService, QuotationPdfService>();
```

When adding new services, always register the interface-to-implementation
mapping in `Program.cs` and verify controller activation resolves all
constructor dependencies.


## Issue 6 — Same DbContext Concurrency Bug Recurring in DashboardService

### Problem
After Prompt #13 (dashboard KPI cards), loading the Dashboard page threw
`InvalidOperationException: A second operation was started on this
context instance before a previous operation completed`. The dashboard
failed to render any KPI values.

### How I Investigated
- Read the stack trace; it pointed to `DashboardService.GetSummaryAsync`
  (line 52) and EF Core's DbContext concurrency guard.
- Opened `DashboardService.cs` and found `Task.WhenAll` running four
  `CountAsync` queries in parallel (TotalProducts, TotalCustomers,
  TotalQuotationsToday, PendingQuotations) — all on the same scoped
  `AppDbContext` instance.
- Recognised this as the identical anti-pattern fixed earlier in
  `SearchService` (Issue 4): EF Core `DbContext` is not thread-safe for
  concurrent operations.

### How AI Helped
- Cursor applied the same fix as SearchService: replace `Task.WhenAll`
  with sequential `await` calls, one count query at a time.
- Scanned the entire codebase for other `Task.WhenAll` usages against
  DbContext — none found beyond this instance.

### What I Validated
- Rebuilt SmeErp.Infrastructure after the change.
- Called `GetSummaryAsync` for company 1 (Sharma Trading) — returned
  4 products, 3 customers, 3 quotations today, 3 pending; no exception.
- Called `GetSummaryAsync` for company 2 (Verma Distributors) — returned
  4 products, 3 customers, 0 quotations today, 0 pending; confirming
  correct, different counts per tenant.
- Dashboard page loads correctly for both admin@sharmatrading.com and
  admin@vermadist.com.

### Final Fix
In `DashboardService.GetSummaryAsync`, replace parallel task execution
with sequential awaits:

```csharp
var totalProducts = await _dbContext.Products.CountAsync(...);
var totalCustomers = await _dbContext.Customers.CountAsync(...);
var quotationsToday = await _dbContext.Quotations.CountAsync(...);
var pendingQuotations = await _dbContext.Quotations.CountAsync(...);
```

Never use `Task.WhenAll` against a single `DbContext` instance. This
pattern has now caused bugs in both SearchService and DashboardService.

