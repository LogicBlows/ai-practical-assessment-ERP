## Prompt #2 — Generate Planning.md Entry for Products/Customers Feature
**Date:** 2026-07-20

**Prompt:**
Append an entry to ai-prompts/planning.md, following the exact same
format as existing entries (## Prompt #N heading, Date, Prompt,
Response summary, Accepted/Changed/Rejected, Commit), documenting this
as 'Prompt #8 — Products and Customers List/Search Pages'. Use the
exact prompt text I gave you at the start of this session as the
'Prompt' field, and use your own summary above as the basis for the
'Response summary' field.

**Response summary:**
Cursor appended the Prompt #8 entry to ai-prompts/planning.md,
documenting the Products and Customers list/search feature. The entry
includes the full original prompt text (ProductsController and
CustomersController with company-scoped keyword search, Application-layer
services returning DTOs, Razor Index views, authenticated nav links, and
[Authorize]), a response summary covering ServiceResult<T>, DTOs,
Infrastructure service implementations, and per-tenant verification, plus
Accepted/Changed/Rejected and a commit message.

**Accepted / Changed / Rejected:**
- Accepted: Prompt #8 planning entry added in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Document Products/Customers list feature in planning.md (prompt #8 entry)"


## Prompt #3 — Generate Planning.md Entry for Quotations Feature
**Date:** 2026-07-20

**Prompt:**
Append an entry to ai-prompts/planning.md, following the exact same
format as existing entries, documenting this as 'Prompt #9 —
Quotation Creation, List, and Detail Flow'. Use the exact prompt text
I gave you at the start of this session as the 'Prompt' field, and use
your own summary above as the basis for the 'Response summary' field.
Additionally note in the summary that manual verification was performed
comparing hand-calculated line/quotation totals against the app's
output, and that cross-tenant isolation was confirmed (Verma's user
sees zero of Sharma's quotations).

**Response summary:**
Cursor appended the Prompt #9 entry to ai-prompts/planning.md,
documenting the Quotation creation, list, and detail feature and its
manual verification. The entry includes the full original prompt text
(IQuotationService/QuotationService with line and quotation total
calculations, QuotationsController with create/list/detail actions,
Razor views with repeatable line items, and authenticated nav link), a
response summary covering DTOs, company-scoped validation, QT-number
generation, and verification notes (hand-calculated totals matched app
output; Verma's user sees zero of Sharma's quotations), plus
Accepted/Changed/Rejected and a commit message.

**Accepted / Changed / Rejected:**
- Accepted: Prompt #9 planning entry added in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Document Quotation create/list/detail feature in planning.md (prompt #9 entry)"


## Prompt #4 — Generate Planning.md and Debugging-Notes Entries for Settings/PDF Feature
**Date:** 2026-07-20

**Prompt:**
Append an entry to ai-prompts/planning.md documenting "Prompt #10 —
Company Settings and Quotation PDF Generation", using the same format
as existing entries. Use the exact prompt text I gave at the start of
this session as the "Prompt" field, and your own summary as the basis
for "Response summary" — additionally note that a font rendering bug
was found (words containing "ti" were corrupted, e.g. "Quotation" ->
"Quotaon") and fixed by explicitly setting FontFamily instead of
relying on default font fallback, and that Settings-to-PDF consistency
was manually verified by changing the company address and color and
confirming the next generated PDF reflected the change.

Also append a new entry to debugging-notes.md as "## Issue 3 — QuestPDF
Font Rendering Corrupted 'ti' Character Sequences", following the
existing template (Problem / How I Investigated / How AI Helped /
What I Validated / Final Fix), documenting this font bug and its fix.

Then append an entry to ai-prompts/documentation.md documenting this
very prompt as "Prompt #4 — Generate Planning.md and Debugging-Notes
Entries for Settings/PDF Feature".

**Response summary:**
Cursor appended three documentation entries for the Company Settings and
Quotation PDF feature. In ai-prompts/planning.md, Prompt #10 was added
with the full original prompt text, a response summary covering
CompanySettingsService, SettingsController/Index view, QuestPDF PDF
generation and download, the font rendering bug fix (Arial FontFamily
on DefaultTextStyle instead of default fallback), and manual verification
that Settings changes (address, PrimaryColor) appear on the next PDF
download. In ai-prompts/debugging-notes.md, Issue 3 documents the
QuestPDF "ti" ligature corruption problem, investigation steps, AI
assistance, validation, and the DefaultTextStyle fix. In
ai-prompts/documentation.md, this meta-documentation prompt was recorded
as Prompt #4.

**Accepted / Changed / Rejected:**
- Accepted: all three documentation entries added in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Document company settings/PDF feature in planning and debugging notes (prompt #10 entry)"


## Prompt #5 — Generate Planning.md Entry for UI Styling Pass
**Date:** 2026-07-20

**Prompt:**
Append an entry to ai-prompts/planning.md documenting "Prompt #11 —
UI Styling Pass (Bootswatch Theme, Card Layouts)", using the same
format as existing entries. Use the exact styling prompt text I gave
at the start of this session as the "Prompt" field, and summarize what
was applied (Bootswatch theme via CDN, PrimaryColor-accented navbar,
striped/hover tables, card-based layouts) as the "Response summary".
Note that all pages were manually verified to still function correctly
after the visual-only change (login, search, quotation creation, PDF
download all confirmed working).

Then append an entry to ai-prompts/documentation.md documenting this
very prompt as "Prompt #5 — Generate Planning.md Entry for UI Styling
Pass".

**Response summary:**
Cursor appended Prompt #11 to ai-prompts/planning.md, documenting the
UI styling pass with the full original prompt text and a response
summary covering the Bootswatch Flatly CDN theme, PrimaryColor-accented
navbar via CompanyBrandingViewComponent, striped/hover list tables, and
card-based layouts on Dashboard, Settings, and Quotation Create/Details,
plus manual verification that login, search, quotation creation, and
PDF download all still work. This meta-documentation prompt was recorded
in ai-prompts/documentation.md as Prompt #5.

**Accepted / Changed / Rejected:**
- Accepted: Prompt #11 planning entry and Prompt #5 documentation entry
  added in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Document UI styling pass in planning.md (prompt #11 entry)"


## Prompt #6 — Generate Planning.md and Debugging-Notes Entries for Global Search Feature
**Date:** 2026-07-20

**Prompt:**
Append an entry to ai-prompts/planning.md documenting "Prompt #12 —
Global Search", using the same format as existing entries. Use the
exact Global Search prompt text I gave at the start of this session as
the "Prompt" field, and note in the "Response summary" that two bugs
were found and fixed during verification: (1) SearchService originally
used Task.WhenAll on a shared DbContext causing an
InvalidOperationException, fixed by running searches sequentially; and
(2) IQuotationPdfService was missing from DI registration in
Program.cs, causing QuotationsController to fail entirely, fixed by
adding the missing AddScoped registration. Note that cross-tenant
isolation was verified after both fixes.

Append two NEW entries to debugging-notes.md — check the file first to
confirm Issues 1-3 already exist and do not duplicate them — add
"## Issue 4 — DbContext Concurrency Exception in Parallel Search
Queries" and "## Issue 5 — Missing DI Registration for
IQuotationPdfService", following the existing template.

Then append an entry to ai-prompts/documentation.md documenting this
prompt itself as "Prompt #6 — Generate Planning.md and Debugging-Notes
Entries for Global Search Feature".

**Response summary:**
Cursor appended Prompt #12 to ai-prompts/planning.md with the full
original global search prompt text and a response summary covering
ISearchService/SearchService, SearchController, grouped Search/Index
view, navbar search form, and the two bugs fixed during verification
(Task.WhenAll DbContext concurrency in SearchService; missing
IQuotationPdfService DI registration), plus cross-tenant isolation
verification. In ai-prompts/debugging-notes.md, Issues 4 and 5 were
added (after confirming Issues 1–3 already existed) documenting the
parallel DbContext search failure and the missing PDF service
registration. This meta-documentation prompt was recorded in
ai-prompts/documentation.md as Prompt #6.

**Accepted / Changed / Rejected:**
- Accepted: Prompt #12 planning entry, Issues 4 and 5 in debugging
  notes, and Prompt #6 documentation entry — all in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Document global search feature in planning and debugging notes (prompt #12 entry)"


## Prompt #7 — Generate Planning.md and Debugging-Notes Entries for Dashboard KPIs Feature
**Date:** 2026-07-20

**Prompt:**
Append an entry to ai-prompts/planning.md documenting "Prompt #13 —
Dashboard KPI Cards", using the same format as existing entries. Use
the exact Dashboard KPIs prompt text I gave at the start of this
session as the "Prompt" field, and note that DashboardService had the
same Task.WhenAll concurrency bug, fixed the same way. Note both seeded
users were verified to show correct, different KPI counts.

Append ONE new entry to debugging-notes.md — check the file first to
confirm Issues 1-5 already exist and do not duplicate them — add
"## Issue 6 — Same DbContext Concurrency Bug Recurring in
DashboardService", following the existing template.

Then append an entry to ai-prompts/documentation.md documenting this
prompt as "Prompt #7 — Generate Planning.md and Debugging-Notes
Entries for Dashboard KPIs Feature".

**Response summary:**
Cursor appended Prompt #13 to ai-prompts/planning.md with the full
original dashboard KPIs prompt text and a response summary covering
IDashboardService/DashboardService, DashboardController/Index view
with four Bootstrap KPI cards, the Task.WhenAll DbContext concurrency
fix in DashboardService, and verification that both seeded users
(Sharma Trading and Verma Distributors) show correct, different counts.
In ai-prompts/debugging-notes.md, Issue 6 was added (after confirming
Issues 1–5 already existed) documenting the recurring parallel
DbContext bug in DashboardService. This meta-documentation prompt was
recorded in ai-prompts/documentation.md as Prompt #7.

**Accepted / Changed / Rejected:**
- Accepted: Prompt #13 planning entry, Issue 6 in debugging notes, and
  Prompt #7 documentation entry — all in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Document dashboard KPI cards feature in planning and debugging notes (prompt #13 entry)"


## Prompt #8 — Generate Planning.md Entry and Test-Results.md for Mandatory xUnit Tests
**Date:** 2026-07-21

**Prompt:**
Append an entry to ai-prompts/planning.md documenting "Prompt #14 —
Mandatory xUnit Tests (Quotation Calculation, Settings Defaults)",
using the same format as existing entries. Use the exact test prompt
text I gave at the start of this session as the "Prompt" field, and
note in the "Response summary" that dotnet test confirmed 2/2 tests
passing (Total: 2, Failed: 0, Succeeded: 2, Duration: 3.6s).

Then append an entry to ai-prompts/documentation.md documenting this
prompt itself as "Prompt #8 — Generate Planning.md Entry and
Test-Results.md for Mandatory xUnit Tests".

**Response summary:**
Cursor appended Prompt #14 to ai-prompts/planning.md with the full
original mandatory xUnit tests prompt text and a response summary
covering QuotationTotalsCalculator extraction, both test classes,
InMemory EF setup, and dotnet test results (2/2 passing, 3.6s duration).
Test names and pass/fail results were already recorded in
test-results.md at the repository root. This meta-documentation prompt
was recorded in ai-prompts/documentation.md as Prompt #8.

**Accepted / Changed / Rejected:**
- Accepted: Prompt #14 planning entry and Prompt #8 documentation entry
  added in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Document mandatory xUnit tests in planning.md (prompt #14 entry)"


## Prompt #9 — Generate README.md
**Date:** 2026-07-24

**Prompt:**
Read the entire codebase (src/ structure, Program.cs, appsettings.json,
migrations) and generate a README.md at the repo root covering:

1. Project overview — one paragraph describing what SmeErp is (small
   business ERP: products, customers, quotations, PDF generation,
   multi-tenant).
2. Tech stack — .NET 6, EF Core 6, SQL Server, Razor MVC, ASP.NET
   Identity, QuestPDF.
3. Prerequisites — .NET 6 SDK, SQL Server (Express or full instance),
   dotnet-ef tool.
4. Setup instructions, step by step:
   - Clone the repo
   - Update the connection string in src/SmeErp.Web/appsettings.json
     (explain the format, no real credentials, just a placeholder like
     "Server=YOUR_SERVER;Database=SmeErpDb;Trusted_Connection=True;TrustServerCertificate=True;")
   - Run `dotnet ef database update --project src/SmeErp.Infrastructure --startup-project src/SmeErp.Web`
   - Run `dotnet run` from src/SmeErp.Web
   - Navigate to the printed localhost URL
5. Seeded login credentials for both demo companies (email/password for
   admin@sharmatrading.com and admin@vermadist.com — note clearly this
   password is a placeholder for local/demo use only).
6. Running tests — `dotnet test` from repo root.
7. Project structure — brief description of the Clean Architecture
   layers (Domain/Application/Infrastructure/Shared/Web) and what lives
   in each.
8. Key features list (multi-tenant isolation, quotation PDF generation,
   settings-driven branding, global search, dashboard KPIs).

Keep it factual and based only on what's actually in the codebase —
don't invent features or steps that don't exist.

**Response summary:**
Cursor read the codebase (src/ structure, Program.cs, appsettings.json,
migrations, IdentitySeeder, launchSettings.json, solution layout, and
test project) and generated README.md at the repository root covering
all eight requested sections: project overview, tech stack table,
prerequisites, step-by-step setup (connection string placeholder,
`dotnet ef database update`, `dotnet run`, localhost URLs), demo login
credentials for both seeded companies with a local/demo-only password
warning, `dotnet test` instructions, Clean Architecture project
structure, and a key-features list limited to implemented functionality.
The README's setup instructions were manually followed step by step on a
fresh check (connection string placeholder, migration command, `dotnet
run`, login with seeded credentials exactly as documented, and `dotnet
test` from repo root) and confirmed to work correctly as written.

**Accepted / Changed / Rejected:**
- Accepted: README.md added at repo root with all eight sections in the
  correct format; setup steps verified manually on a fresh check.
- Changed: none.
- Rejected: none.

**Commit:** "Add README.md with setup instructions and project overview (prompt #9 entry)"


## Prompt #11 — Generate api-contract.md
**Date:** 2026-07-24

**Prompt:**
Read the controllers currently implemented in SmeErp.Web/Controllers
(AccountController, DashboardController, ProductsController,
CustomersController, QuotationsController, SettingsController,
SearchController) and the Application-layer services they call. Draft
api-contract.md documenting each controller action as an endpoint:

1. Route and HTTP method
2. Purpose (one sentence)
3. Request inputs (query params, form fields, route params)
4. Response/redirect behavior
5. Authorization requirements ([Authorize] and any role restrictions)
6. Validation rules enforced

Group by controller. Note explicitly that this project uses MVC
controllers rather than a separate REST API, so "endpoints" here refer
to MVC actions rendering Razor views or redirecting, not JSON responses
(except where noted, e.g. if any AJAX endpoints exist).

Base this strictly on the actual controller definitions and
Application-layer services in the codebase — do not invent endpoints or
behaviors that don't exist.

**Response summary:**
Cursor read all seven controllers in SmeErp.Web/Controllers, their
view models (LoginViewModel, CreateQuotationViewModel,
CompanySettingsViewModel), and the Application-layer services they call
(IDashboardService, IProductService, ICustomerService, IQuotationService,
ICompanySettingsService, IQuotationPdfService, ISearchService,
ICurrentCompanyService). Generated api-contract.md at the repository
root documenting 15 MVC actions grouped by controller, each with route,
HTTP method, purpose, inputs, response/redirect behavior, authorization,
and validation rules (controller, data annotations, and service layer).
The document explicitly states the project uses MVC rather than a REST
API, notes no JSON/AJAX endpoints exist, identifies DownloadPdf as the
only non-HTML response (PDF file download), and records that no role-
based restrictions are enforced beyond [Authorize]. Content was checked
against actual controller code for accuracy.

**Accepted / Changed / Rejected:**
- Accepted: api-contract.md added at repo root with all seven controllers
  documented in the correct format; content verified against controller
  source.
- Changed: none.
- Rejected: none.

**Commit:** "Add api-contract.md documenting MVC controller actions (prompt #11 entry)"


## Prompt #12 — Generate design-notes.md
**Date:** 2026-07-24

**Prompt:**
Based on the current solution structure (Clean Architecture layers:
Domain, Application, Infrastructure, Shared, Web), the multi-tenant
CompanyId pattern, ASP.NET Identity setup, the DB-stored JWT signing
key service, and the QuestPDF/Settings integration, draft
design-notes.md covering:

1. Architecture Overview — the 5-layer structure and why (separation
   of concerns, no business logic in controllers/views, no direct
   DbContext access outside Infrastructure).
2. Frontend Design — Razor MVC with Bootswatch theming, PrimaryColor
   driven from company settings.
3. Backend Design — Application-layer services returning ServiceResult<T>,
   controllers as thin orchestrators.
4. Database Design — SQL Server via EF Core, multi-tenant isolation via
   CompanyId, indexing strategy.
5. Validation Strategy — where validation happens (Application layer
   services, not controllers or views).
6. Error Handling Strategy — how failures are surfaced (ServiceResult
   failure messages, ModelState errors, NotFound/Challenge/Error view
   responses, and UseExceptionHandler for unhandled exceptions).

Base this strictly on the actual solution structure and patterns in the
codebase — do not invent architectural decisions that don't exist.

**Response summary:**
Cursor read the solution structure, ServiceResult<T> pattern, controllers,
view models, CompanyBrandingViewComponent, Program.cs (Identity, DI,
exception handler), SigningKeyService/Seeder, QuotationPdfService, and
Infrastructure service implementations. Generated design-notes.md at the
repository root covering all six requested sections: architecture overview
(five-layer Clean Architecture, dependency direction, DbContext isolation,
Identity and signing-key current state), frontend design (Razor MVC,
Bootswatch Flatly, PrimaryColor-driven navbar and PDF branding via
CompanyBrandingViewComponent), backend design (service interfaces,
ServiceResult<T>, thin controllers, DTO/view-model split), database design
(EF Core, CompanyId multi-tenancy, indexing, delete behaviors, seed
strategy), validation strategy (business rules in services, data
annotations on view models, QuotationTotalsCalculator), and error handling
(ServiceResult failure paths, controller response mapping, exception
handler). Added a key integration points table and links to README.md,
data-model.md, and api-contract.md.

**Accepted / Changed / Rejected:**
- Accepted: design-notes.md added at repo root with all six sections in
  the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Add design-notes.md covering architecture and design patterns (prompt #12 entry)"


## Prompt #13 — Generate ui-flow.md
**Date:** 2026-07-24

**Prompt:**
Based on the actual pages and navigation in SmeErp.Web (Login,
Dashboard, Products, Customers, Quotations Index/Create/Details,
Settings, Search), draft ui-flow.md covering:

1. An overview of the navigation structure (top nav links, what's
   visible only to authenticated users).
2. The login flow: unauthenticated user -> Login page -> Dashboard on
   success.
3. For each major page, a brief flow description: how the user gets
   there, what they can do, where actions lead (e.g. Quotations Index
   -> click "New Quotation" -> Create form -> submit -> redirects to
   Details page; Details page -> "Download PDF" -> streams PDF file).
4. The Global Search flow: search box in nav -> results page grouped
   by type -> click a result -> navigates to relevant page.
5. Note where multi-tenant scoping affects what a user sees (e.g. two
   different users see completely different Products/Customers/
   Quotations lists based on their company).

Keep this as a clear, readable flow description (numbered steps or
simple arrows are fine) rather than requiring an actual diagram image.
Base it strictly on real pages/actions that exist in the codebase.

**Response summary:**
Cursor read _Layout.cshtml (navbar links, search form, sign-in/sign-out),
and the Razor views for Login, Dashboard, Products, Customers, Quotations
(Index/Create/Details), Settings, and Search, plus controller redirect
behavior. Generated ui-flow.md at the repository root covering all five
requested sections: navigation structure (authenticated vs unauthenticated
items, pages not in main nav), login and logout flows, per-page flow
descriptions with arrival paths and action outcomes (including quotation
create → details → PDF download and settings save redirect), global search
flow (keyword length rules, grouped results, destination URLs from
SearchService), and multi-tenant scoping with a Sharma vs Verma seeded-
data example. Added an end-to-end quotation walkthrough and links to
related documentation. Used numbered steps and arrow flows only — no
diagram image.

**Accepted / Changed / Rejected:**
- Accepted: ui-flow.md added at repo root with all five sections in the
  correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Add ui-flow.md documenting page navigation and user flows (prompt #13 entry)"


## Prompt #14 — Generate acceptance-criteria.md
**Date:** 2026-07-24

**Prompt:**
Read the Core Acceptance Criteria for Option 3 (.NET Full-Stack — SME
ERP) from the assessment requirements, and cross-reference each item
against what has actually been implemented and verified in this
codebase (controllers, views, tests, migrations, debugging-notes.md).

Draft acceptance-criteria.md as a checklist matching this exact
structure:

## Core
- [ ] A user can log in with seeded credentials
- [ ] A user can list and search products from the database
- [ ] A user can list customers from the database
- [ ] A user can create a quotation with multiple line items via the UI
- [ ] A user can view the quotation list and open a detail view
- [ ] A user can download/print a quotation PDF
- [ ] PDF company address, GSTIN, terms, and accent color come from
      Settings (not hardcoded)
- [ ] Changing Settings updates the next PDF output
- [ ] Global search returns relevant products and customers (and
      quotations if implemented)
- [ ] Data persists after application restart
- [ ] Backend validation rejects invalid quotations (missing customer,
      zero quantity, etc.)
- [ ] UI shows validation and error states clearly
- [ ] No secrets committed to the repository
- [ ] README setup instructions work on a clean machine
- [ ] Mandatory xUnit tests pass

For each item, mark [x] if genuinely implemented and verified (based on
what's actually in this codebase and what was manually tested during
development), or leave [ ] with a brief note if not done or only
partially done. Do not mark something [x] unless there is real evidence
it works (e.g. was tested in this session's history).

## Validation
List the specific validation rules actually implemented (e.g. quantity
> 0, customer must belong to company, product must belong to company).

## Error Handling
List the specific error/failure states actually implemented (e.g.
ServiceResult failure messages, [Authorize] redirects).

## Testing
List what's actually tested (the 2 mandatory xUnit tests) and what
isn't (integration tests, edge cases) — be honest about gaps.

## Documentation
List which lifecycle documents are complete as of this point.

**Response summary:**
Cursor cross-referenced all 15 Core acceptance criteria against
controllers, views, Application/Infrastructure services, EF Core
migrations, xUnit tests, debugging-notes.md, ai-prompts/planning.md
manual verification notes, and test-results.md. Generated
acceptance-criteria.md at the repository root with all five sections.
Each Core checklist item was manually reviewed against actual testing
evidence (planning prompts #6–#13, debugging-notes Issues 1–6,
test-results.md, and a fresh dotnet test run confirming 2/2 passing)
before being marked [x]; items include per-criterion evidence notes
where verification was manual vs automated. Validation section lists
rules from QuotationService, view-model annotations, SearchService, and
Identity. Error Handling section documents ServiceResult, ModelState,
NotFound, Challenge, and Error view patterns. Testing section lists the
two mandatory xUnit tests and honest gaps (no integration tests,
validation rejection paths untested). Documentation section lists all
complete lifecycle documents and notes test-strategy.md is referenced
but missing.

**Accepted / Changed / Rejected:**
- Accepted: acceptance-criteria.md added at repo root with all five
  sections; each Core item reviewed against evidence before marking
  complete.
- Changed: none.
- Rejected: none.

**Commit:** "Add acceptance-criteria.md cross-referencing Core criteria against implementation (prompt #14 entry)"


## Prompt #15 — Generate requirements-analysis.md
**Date:** 2026-07-24

**Prompt:**
Draft requirements-analysis.md for the SmeErp project (Option 3: .NET
Full-Stack SME ERP), covering:

## Selected Project Option
.NET Full-Stack — SME ERP (Inventory Management)

## My Understanding
[Leave this section as a placeholder marked "TO BE FILLED IN BY
DEVELOPER" — this needs to be written in the developer's own words,
not generated]

## Functional Requirements
List the actual functional requirements as implemented: authentication,
multi-tenant company isolation, product/customer list+search,
quotation creation with line-item calculation, quotation PDF
generation, company settings management, global search, dashboard KPIs.

## Non-Functional Requirements
Based on what was actually built: data isolation between tenants,
no secrets in source control, responsive validation, calculation
correctness (verified via manual hand-calculation and unit tests).

## Assumptions
List assumptions actually made during development: one user per
company (no multi-user-per-company support), no self-service tenant
registration (companies/users are seeded by the developer), single
role tier per company (Proprietor) is sufficient for Core scope.

## Clarifications (questions for a product owner)
List 2-3 realistic open questions this project would raise for a real
product owner (e.g. should multiple users per company be supported?
should quotations be editable after creation? should there be an
approval workflow before a quotation is finalized?).

## Edge Cases
List edge cases actually handled or considered: company with no
settings yet (defaults), quotation with zero line items (rejected),
customer/product belonging to a different company (rejected),
duplicate signing key generation on startup (guarded against).

**Response summary:**
Cursor generated requirements-analysis.md at the repository root
covering all seven sections: selected project option, functional
requirements (all implemented Core features), non-functional
requirements (tenant isolation, no secrets, validation, calculation
correctness), development assumptions, three product-owner clarification
questions, and edge cases handled in the codebase (with references to
services, tests, and debugging-notes.md). The "My Understanding"
section was left as an explicit placeholder ("TO BE FILLED IN BY
DEVELOPER") for the developer to complete personally in their own
words — not AI-generated. Added links to related lifecycle documents.

**Accepted / Changed / Rejected:**
- Accepted: requirements-analysis.md added at repo root with all
  sections except "My Understanding", which remains a developer-
  written placeholder.
- Changed: none.
- Rejected: none.

**Commit:** "Add requirements-analysis.md for Option 3 SME ERP scope (prompt #15 entry)"


## Prompt #16 — Generate implementation-plan.md
**Date:** 2026-07-24

**Prompt:**
Based on the actual sequence of work done in this project (visible in
ai-prompts/planning.md and the Git commit history), draft
implementation-plan.md covering:

## Overview
Brief summary of the build approach: incremental, one feature per
branch/PR, with manual verification before each merge.

## Task Breakdown
List the actual sequence of work completed, in order: solution
scaffold, domain entities, DbContext/migrations, seed data,
authentication/roles/current-company resolution, DB-stored JWT signing
key, Products/Customers list+search, Quotation creation/list/detail,
Company Settings + PDF generation, UI styling pass, Global Search,
Dashboard KPIs, mandatory xUnit tests.

## Milestones
Group the above into logical milestones (e.g. "Foundation" =
scaffold+entities+DB+seed; "Auth & Multi-tenancy" = auth+roles+signing
key; "Core Business Features" = products/customers/quotations/settings/
PDF; "Polish & Verification" = styling/search/dashboard/tests).

## AI Usage Plan
Describe the actual prompting approach used: one scoped prompt per
feature, explicit architectural constraints given upfront in the first
prompt, manual verification (running the app, checking SSMS, hand-
calculating totals) before accepting any feature as complete, and
documentation generated via Cursor but reviewed/edited before commit.

## Risks
List real risks that were relevant during development: DbContext
concurrency issues with parallel async calls (which actually
materialized twice — see debugging-notes.md Issues 4 and 6), missing
DI registrations causing silent controller failures, font rendering
issues in generated PDFs, incomplete AI deliverables requiring follow-up
prompts, and stale dotnet processes blocking builds.

**Response summary:**
Cursor read ai-prompts/planning.md (Prompts #1–#14), Git commit history
(chronological log and feature branch PRs #1–#7), and debugging-notes.md
(Issues 1–6). Generated implementation-plan.md at the repository root
covering all five sections: overview (incremental one-feature-per-PR
approach with manual verification), task breakdown table mapping each
prompt to commits and PR branches, four milestones (Foundation, Auth &
Multi-Tenancy, Core Business Features, Polish & Verification), AI usage
plan (fixed constraints in Prompt #1, scoped prompts, verification
checklist, reviewed documentation), and risks table documenting issues
that materialized (DbContext concurrency in SearchService and
DashboardService, missing IQuotationPdfService DI registration, QuestPDF
font ligature corruption, incomplete Prompt #4 seed data, stale dotnet
processes) plus future risks noted. Post-Core documentation commits
listed separately.

**Accepted / Changed / Rejected:**
- Accepted: implementation-plan.md added at repo root with all five
  sections in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Add implementation-plan.md documenting build sequence and milestones (prompt #16 entry)"


## Prompt #17 — Generate test-strategy.md
**Date:** 2026-07-24

**Prompt:**
Based on the actual test implemented (SmeErp.Application.Tests) and
the project's testing history (test-results.md, debugging-notes.md),
draft test-strategy.md covering:

## Test Scope
What is covered by testing in this project: quotation calculation
correctness, settings-defaults behavior. What is NOT covered:
integration/end-to-end tests, UI tests, load/performance tests.

## Unit Tests
Describe the two mandatory unit tests actually implemented: the
quotation calculation test (isolated from EF Core via
QuotationTotalsCalculator) and the settings defaults test.

## Component Tests
State honestly that no component-level tests exist for controllers or
views in this project (Core scope did not require this).

## API / Integration Tests
State honestly that no integration tests using WebApplicationFactory or
similar exist yet — note this is listed as Stretch in the assessment
requirements, not required for Core.

## Edge Case Tests
List edge cases considered but not automated: quotation with
zero-quantity line items (rejected by validation, tested manually not
via automated test), cross-tenant data access (verified manually across
every feature via SSMS/UI, not via automated integration test).

## Tests Not Covered (and why)
Be explicit and honest: authentication flow, role-based authorization,
cross-tenant isolation, PDF generation correctness, and search/dashboard
concurrency behavior were all verified through manual testing during
development (documented in debugging-notes.md) but are not covered by
automated tests. This reflects the trimmed Core scope's 8-12 hour time
budget, with the mandatory test tier satisfied and remaining time
allocated to lifecycle documentation as prioritized by the assessment
guidelines.

**Response summary:**
Cursor read SmeErp.Application.Tests (QuotationCalculationTests,
CompanySettingsDefaultsTests), QuotationTotalsCalculator, test-results.md
(2/2 passing), and debugging-notes.md. Generated test-strategy.md at the
repository root covering all six sections: test scope (two automated areas
vs explicit gaps), detailed descriptions of both mandatory unit tests,
honest statements that no component or integration tests exist (Stretch
tier), edge cases table distinguishing automated vs manual verification,
and Tests Not Covered with rationale (8–12 hour Core budget, mandatory
tier satisfied, manual verification documented in planning.md and
debugging-notes.md). Resolves the forward reference in test-results.md,
which previously pointed to a missing test-strategy.md file.

**Accepted / Changed / Rejected:**
- Accepted: test-strategy.md added at repo root with all six sections
  in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Add test-strategy.md documenting test scope and coverage gaps (prompt #17 entry)"


## Prompt #18 — Generate code-review-notes.md and review-fixes.md
**Date:** 2026-07-24

**Prompt:**
Based on the actual bugs found and fixed during this project (see
debugging-notes.md Issues 1-6), and the pattern of reviewing Cursor's
generated code before committing (checking diffs, verifying against
manual testing, catching incomplete work), draft two files:

code-review-notes.md:

## AI-Assisted Review Summary
Describe the actual review process used throughout: after each Cursor
prompt, code was reviewed for (a) architectural compliance (no
DbContext in controllers, ServiceResult<T> pattern followed), (b)
correctness against manually verified data/calculations, and (c)
completeness (checking the database directly rather than trusting a
"success" message, which caught several under-delivered prompts).

## My Review Observations
List specific real observations made during review: Cursor initially
seeded only Company data when four entity types were requested
(caught via SSMS inspection); Cursor used Task.WhenAll against a
shared DbContext in two separate features (SearchService and
DashboardService), a pattern that should have been avoided from a
correctness standpoint; a DI registration was missing for
IQuotationPdfService, breaking an entire controller; a font library
default caused silent character corruption in generated PDFs.

## Changes Made After Review
List the actual fixes applied: added missing seed data for Product/
Customer/CompanySetting; replaced Task.WhenAll with sequential awaits
in both SearchService and DashboardService; added the missing
AddScoped registration for IQuotationPdfService in Program.cs; set an
explicit FontFamily and disabled standard ligatures in
QuotationPdfService.

## Suggestions Rejected (and why)
Describe any Cursor suggestions or defaults not accepted: kept MVC
controllers instead of adding a separate Web API project (assessed as
unnecessary scope for Core requirements); did not implement public
self-service tenant registration despite initial interest, since it
exceeded Core/Stretch scope and would have displaced time needed for
lifecycle documentation.

review-fixes.md:

For each fix listed above, document: what was found, where in the
repo it was fixed (reference the actual commit messages, e.g. "Add
Global Search with DbContext concurrency and DI registration fixes"),
and how it was verified afterward (re-running the feature manually,
re-checking cross-tenant isolation).

**Response summary:**
Cursor read debugging-notes.md (Issues 1–6), ai-prompts/planning.md,
and Git commit history. Generated code-review-notes.md and
review-fixes.md at the repository root. code-review-notes.md covers
the three-part review process (architectural compliance, manual
correctness verification, SSMS/database completeness checks), six
specific review observations, five code fixes applied after review,
and rejected suggestions (no separate Web API, no self-service
registration, no parallel Task.WhenAll, no default QuestPDF font).
review-fixes.md documents each fix with what was found, commit hash
and message (226516f seed data; 7c20d79 search/dashboard concurrency
and DI registration; 8e2f3b5 PDF font fix), affected files, and
verification steps including cross-tenant isolation checks; also notes
environmental Issues 1–2 (stale dotnet processes). Both files
cross-reference debugging-notes.md and each other.

**Accepted / Changed / Rejected:**
- Accepted: code-review-notes.md and review-fixes.md added at repo
  root with all requested sections in the correct format.
- Changed: none.
- Rejected: none.

**Commit:** "Add code-review-notes.md and review-fixes.md documenting review process and fixes (prompt #18 entry)"

