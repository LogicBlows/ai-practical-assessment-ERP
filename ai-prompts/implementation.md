# SmeErp Implementation Prompt Log

Copies of feature build-out prompts from `ai-prompts/planning.md`.
Original numbering preserved. See `planning.md` for the complete chronological log.

---

## Prompt #6 — Authentication, Roles, and Current-Company Resolution
**Date:** 2026-07-19

**Prompt:**
Add authentication and multi-tenant user scoping to the SmeErp application.

1. Extend ApplicationUser (already extending IdentityUser) with a
   CompanyId (int) property — the company this user belongs to.

2. Seed two roles via RoleManager: "Admin" and "Proprietor".

3. Seed two ApplicationUsers via UserManager, each linked to one of the
   already-seeded companies:
   - admin@sharmatrading.com, CompanyId = 1 (Sharma Trading Co.), role "Proprietor"
   - admin@vermadist.com, CompanyId = 2 (Verma Distributors), role "Proprietor"
   Use a seeded password that's clearly a placeholder (e.g. "Passw0rd!123")
   and note in a code comment that this is for local/demo use only.

4. Create an ICurrentCompanyService interface and implementation in
   SmeErp.Infrastructure that, given the currently authenticated user
   (via IHttpContextAccessor), returns that user's CompanyId. Register
   it in DI as scoped.

5. Implement a login/logout flow using ASP.NET Identity's
   SignInManager and Razor views (simple email + password form, no
   registration page — users are seeded, not self-registered).
   On successful login, redirect to a placeholder /Dashboard page.

6. Add [Authorize] to a placeholder DashboardController so unauthenticated
   users are redirected to login.

Do not implement the DB-stored JWT signing key yet — that's a separate
step. Do not build Products/Customers/Quotations pages yet — just the
login flow and the CurrentCompanyService plumbing. Run
'dotnet ef migrations add SeedUsersAndRoles' and apply it after the
role/user seeding is added.

**Response summary:**
Cursor extended ApplicationUser with CompanyId, seeded roles (Admin,
Proprietor) and two users (admin@sharmatrading.com -> Company 1,
admin@vermadist.com -> Company 2), created ICurrentCompanyService
resolving the logged-in user's CompanyId via IHttpContextAccessor,
and implemented login/logout with a placeholder [Authorize] Dashboard.
Generated and applied migration 20260719192330_SeedUsersAndRoles.
Verified by logging in as both seeded accounts — dashboard correctly
displayed "Current company ID: 1" and "2" respectively, confirming
correct per-user tenant resolution.

**Accepted / Changed / Rejected:**
- Accepted: full auth flow, role seeding, CurrentCompanyService.
- Changed: none — ran smoothly once the port-conflict issue (unrelated
  to this prompt) was resolved.
- Rejected: none.

**Commit:** "Add auth, seeded users/roles, and current-company resolution (prompt #6)"


## Prompt #8 — Products and Customers List/Search Pages
**Date:** 2026-07-20

**Prompt:**
Build the Products and Customers list pages for the SmeErp application,
scoped to the current user's company via ICurrentCompanyService.

1. Create a ProductsController (SmeErp.Web) with an Index action that
   lists all Products where CompanyId matches the current user's
   company, and supports a keyword search query parameter matching
   against Name, Sku, or Barcode.

2. Create a CustomersController similarly, listing Customers scoped to
   the current company, with keyword search against Name or Code.

3. Both actions should go through an Application-layer service
   (e.g. IProductService, ICustomerService in SmeErp.Application) that
   accepts the CompanyId and search term, returning DTOs — no direct
   DbContext or repository calls from the controllers.

4. Create simple Razor views (Index.cshtml for each) showing a table
   of results and a search input box that submits via GET.

5. Add both to the main navigation in _Layout.cshtml, visible only to
   authenticated users.

6. Add [Authorize] to both controllers.

Do not implement create/edit forms yet — list + search only, per Core
scope. Do not implement Quotations yet.

**Response summary:**
Cursor introduced ServiceResult<T> in SmeErp.Application along with
ProductListItemDto and CustomerListItemDto, IProductService and
ICustomerService interfaces, and ProductService/CustomerService
implementations in Infrastructure (EF queries filtered by CompanyId with
optional keyword search, projected to DTOs). ProductsController and
CustomersController resolve the current company via ICurrentCompanyService,
delegate to the Application services, and return Index views with GET
search forms and Bootstrap result tables. Products and Customers nav
links were added to _Layout.cshtml for authenticated users only. Both
controllers are decorated with [Authorize]. Verified by build
(dotnet build SmeErp.sln — 0 warnings) and manual testing: Sharma
Trading sees 4 hardware products and 3 Jaipur customers; Verma
Distributors sees 4 stationery products and 3 Pune customers, confirming
correct per-tenant scoping.

**Accepted / Changed / Rejected:**
- Accepted: Application-layer services with DTOs, company-scoped list +
  search, Razor views, nav links, [Authorize] on both controllers.
- Changed: none.
- Rejected: none.

**Commit:** "Add Products and Customers list/search pages (prompt #8)"


## Prompt #9 — Quotation Creation, List, and Detail Flow
**Date:** 2026-07-20

**Prompt:**
Build the Quotation creation, list, and detail flow for the SmeErp
application, scoped to the current user's company via
ICurrentCompanyService.

1. In SmeErp.Application, create DTOs:
   - QuotationLineInputDto (ProductId, Quantity, UnitPrice, DiscountPercent)
   - CreateQuotationRequestDto (CustomerId, QuotationDate, ValidUntil,
     Notes, List<QuotationLineInputDto> Lines)
   - QuotationListItemDto (Id, QuotationNumber, CustomerName,
     QuotationDate, TotalAmount)
   - QuotationDetailDto (all Quotation fields plus Customer name and a
     list of line details with product name, quantity, unit price,
     discount, GST percent, line tax amount, line total amount)

2. Create IQuotationService/QuotationService in Application/Infrastructure
   with:
   - CreateAsync(companyId, CreateQuotationRequestDto): validates
     CustomerId belongs to the same company, validates every line has
     Quantity > 0 and a valid ProductId belonging to the same company,
     then for each line calculates:
       lineSubtotal = Quantity * UnitPrice
       lineDiscount = lineSubtotal * (DiscountPercent / 100)
       lineTaxableAmount = lineSubtotal - lineDiscount
       lineTaxAmount = lineTaxableAmount * (GstPercent / 100)  [GstPercent
         comes from the Product, not user input]
       lineTotalAmount = lineTaxableAmount + lineTaxAmount
     Then sums across all lines for the Quotation's SubTotal,
     DiscountAmount, TaxAmount, TotalAmount. Auto-generates
     QuotationNumber (e.g. "QT-{CompanyId}-{sequential or timestamp-based
     number}"). Returns ServiceResult<int> (the new Quotation's Id) or
     a validation failure with a clear message.
   - GetListAsync(companyId): returns QuotationListItemDto list, scoped
     to company, newest first.
   - GetDetailAsync(companyId, quotationId): returns QuotationDetailDto,
     or a not-found result if the quotation doesn't belong to this company.

3. Create QuotationsController (SmeErp.Web), [Authorize]:
   - Index: list view
   - Create (GET): shows a form to select customer and add line items
     (populate Customer and Product dropdowns scoped to current company)
   - Create (POST): calls CreateAsync, redirects to Detail on success,
     redisplays the form with validation errors on failure
   - Details(id): shows the quotation detail view

4. Create Razor views: Quotations/Index.cshtml (list table),
   Quotations/Create.cshtml (form with a dynamic-feeling but simple
   repeatable line-item section — plain HTML/JS is fine, no heavy
   framework needed), Quotations/Details.cshtml (read-only detail view
   showing all lines and totals).

5. Add Quotations to the navigation.

Do not implement PDF generation yet — that is a separate step. Do not
allow editing an existing quotation after creation for now (Core scope
is create + view, not edit).

**Response summary:**
Cursor added quotation DTOs (QuotationLineInputDto,
CreateQuotationRequestDto, QuotationListItemDto, QuotationLineDetailDto,
QuotationDetailDto) and IQuotationService in SmeErp.Application, with
QuotationService in Infrastructure implementing CreateAsync (company-scoped
validation, per-line discount/tax calculations using product GST%,
quotation-level totals, and QT-{CompanyId}-{sequential} numbering),
GetListAsync (newest first), and GetDetailAsync (not-found when outside
the tenant). QuotationsController ([Authorize]) orchestrates list,
create GET/POST, and detail actions; Razor views provide a list table,
a create form with repeatable line items (plain JS add/remove and
product price auto-fill), and a read-only detail page with line and
totals breakdown. Quotations nav link added for authenticated users.
Verified by build (dotnet build SmeErp.sln — 0 warnings) and manual
testing: hand-calculated line and quotation totals matched the app's
output on the detail view, and cross-tenant isolation was confirmed
(Verma's user sees zero of Sharma's quotations).

**Accepted / Changed / Rejected:**
- Accepted: full create + list + detail flow, Application-layer service
  with ServiceResult<T>, company-scoped validation and calculations,
  Razor views, nav link, [Authorize] on controller.
- Changed: none.
- Rejected: none.

**Commit:** "Add quotation create, list, and detail flow (prompt #9)"


## Prompt #12 — Global Search
**Date:** 2026-07-20

**Prompt:**
Build global search for the SmeErp application, scoped to the current
user's company via ICurrentCompanyService.

1. Create ISearchService/SearchService in Application/Infrastructure
   with SearchAsync(companyId, keyword) that, for keywords of at least
   2 characters, queries:
   - Products (match Name, Sku, or Barcode)
   - Customers (match Name or Code)
   - Quotations (match QuotationNumber, or the linked Customer's Name)
   Returns a combined result DTO with separate lists (or a unified list
   tagged by type), each item including enough info to link to its
   detail/edit page (id, display name/title, type, and a route/URL).

2. Add a search box to the top navigation in _Layout.cshtml (a simple
   GET form submitting to a new SearchController).

3. Create SearchController ([Authorize]) with an Index action that
   calls SearchAsync and returns a results view.

4. Create Search/Index.cshtml showing results grouped by type
   (Products, Customers, Quotations), each linking to its respective
   detail page. Show a friendly "no results" message if nothing matches
   or if the keyword is under 2 characters.

Keep this simple — no autocomplete/AJAX needed, a full page reload on
search submit is fine for Core scope.

**Response summary:**
Cursor added SearchResultType, SearchResultItemDto, and
GlobalSearchResultDto in SmeErp.Application, with ISearchService and
SearchService in Infrastructure implementing company-scoped search
across Products (Name/Sku/Barcode), Customers (Name/Code), and
Quotations (QuotationNumber or linked Customer name), returning grouped
results with display names, subtitles, and URLs. SearchController
([Authorize]) and Search/Index.cshtml show results by type with friendly
messages for short keywords and no matches; a GET search form was added
to the navbar in _Layout.cshtml for authenticated users. ISearchService
was registered in Program.cs. Two bugs were found and fixed during
verification: (1) SearchService originally used Task.WhenAll on a shared
DbContext, causing an InvalidOperationException ("A second operation was
started on this context instance"); fixed by running the three searches
sequentially with separate await calls. (2) IQuotationPdfService was
missing from DI registration in Program.cs, causing QuotationsController
to fail entirely; fixed by adding
`builder.Services.AddScoped<IQuotationPdfService, QuotationPdfService>()`.
Cross-tenant isolation was verified after both fixes (each company's
search results only include its own products, customers, and quotations).

**Accepted / Changed / Rejected:**
- Accepted: global search service, SearchController, grouped results
  view, navbar search form, sequential DbContext queries, DI registration
  fix for IQuotationPdfService.
- Changed: SearchService refactored from Task.WhenAll to sequential
  awaits after concurrency bug; Program.cs updated with missing PDF
  service registration discovered during search/PDF verification.
- Rejected: none.

**Commit:** "Add global search across products, customers, and quotations (prompt #12)"


## Prompt #13 — Dashboard KPI Cards
**Date:** 2026-07-20

**Prompt:**
Build the minimal dashboard KPI cards for the SmeErp application,
replacing the current placeholder Dashboard content, scoped to the
current user's company.

1. Create IDashboardService/DashboardService with GetSummaryAsync
   (companyId) returning: TotalProducts, TotalCustomers,
   TotalQuotationsToday (QuotationDate == today), and
   PendingQuotations (define as quotations with ValidUntil >= today,
   representing still-valid/open quotes) — all counted via real
   database queries, not hardcoded.

2. Update DashboardController's Index action to call this service and
   pass the summary to the view.

3. Update Dashboard/Index.cshtml to show these four values as
   Bootstrap card components in a responsive row, with clear labels
   (e.g. "Total Products", "Total Customers", "Quotations Today",
   "Pending Quotations"), replacing the old "Current company ID: X"
   placeholder text (that CompanyId debug info can be removed now,
   its job is done).

Keep it simple — four cards, real counts, no charts (charts are Stretch,
not Core).

**Response summary:**
Cursor added DashboardSummaryDto and IDashboardService in
SmeErp.Application, with DashboardService in Infrastructure implementing
GetSummaryAsync via real EF Core CountAsync queries scoped by CompanyId:
total products, total customers, quotations created today (QuotationDate
within today's date range), and pending quotations (ValidUntil >= today).
DashboardController was updated to call the service and pass a
DashboardIndexViewModel; Dashboard/Index.cshtml now shows four responsive
Bootstrap KPI cards and the placeholder "Current company ID" debug text
was removed. IDashboardService was registered in Program.cs. During
verification, DashboardService had the same Task.WhenAll concurrency bug
as SearchService — four parallel CountAsync calls against a shared
DbContext caused InvalidOperationException; fixed by replacing Task.WhenAll
with sequential await calls. Both seeded users were verified to show
correct, different KPI counts: admin@sharmatrading.com (4 products, 3
customers, 3 quotations today, 3 pending) and admin@vermadist.com (4
products, 3 customers, 0 quotations today, 0 pending).

**Accepted / Changed / Rejected:**
- Accepted: dashboard service with real DB counts, KPI card layout,
  removal of CompanyId placeholder, sequential DbContext queries after
  concurrency fix.
- Changed: DashboardService refactored from Task.WhenAll to sequential
  awaits after the same DbContext concurrency exception seen in
  SearchService.
- Rejected: none.

**Commit:** "Add dashboard KPI cards with company-scoped counts (prompt #13)"


## Prompt #11 — UI Styling Pass (Bootswatch Theme, Card Layouts)
**Date:** 2026-07-20

**Prompt:**
The current UI uses the default unstyled Bootstrap MVC template look.
Apply a clean, professional visual pass without changing any
functionality or routes:

1. Replace the default Bootstrap CSS with a Bootswatch theme (use the
   "Cosmo" or "Flatly" theme — pick one, load it via CDN link in
   _Layout.cshtml instead of the local bootstrap.css).
2. Improve the navbar: make it use the current company's PrimaryColor
   setting as an accent (read from ICompanySettingsService), add
   reasonable padding/spacing.
3. Style the Products, Customers, and Quotations list tables with
   Bootstrap's table-striped and table-hover classes for readability.
4. Give the Dashboard, Settings, and Quotation Create/Details pages
   card-based layouts (Bootstrap .card component) instead of plain
   unstyled divs, with reasonable spacing.
5. Do not change any controller logic, routes, or functionality —
   this is a visual-only pass.

**Response summary:**
Cursor applied a visual-only styling pass across SmeErp.Web with no
controller, route, or functionality changes. Replaced the local
bootstrap.min.css link in _Layout.cshtml with the Bootswatch Flatly
theme (Bootstrap 5.1.3) loaded via jsDelivr CDN; removed conflicting
btn-primary overrides from the scoped layout CSS. Added a
CompanyBrandingViewComponent that reads the current company's
PrimaryColor (and company name) via ICompanySettingsService and
ICurrentCompanyService, exposing a CSS variable used to accent the
navbar; improved navbar padding, dark styling, and collapsible layout.
Products, Customers, and Quotations list tables use table-striped and
table-hover (with table-light headers for contrast). Dashboard,
Settings, and Quotation Create/Details pages were wrapped in Bootstrap
.card layouts with headers, padding, and light shadows via site.css
(.page-card, light page background). All pages were manually verified
to still function correctly after the visual-only change — login,
search, quotation creation, and PDF download were all confirmed working.

**Accepted / Changed / Rejected:**
- Accepted: Bootswatch Flatly CDN theme, PrimaryColor-accented navbar,
  striped/hover list tables, card-based layouts on Dashboard/Settings/
  Quotation Create/Details, visual-only scope with no controller changes.
- Changed: none to application logic; ViewComponent added for navbar
  branding (view-layer only).
- Rejected: none.

**Commit:** "Apply Bootswatch theme and card-based UI styling (prompt #11)"
