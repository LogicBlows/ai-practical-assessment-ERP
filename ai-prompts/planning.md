## Prompt #1 — Solution Scaffold
**Date:** 2026-07-19
**Prompt:**
I'm building a small ERP (inventory/quotation management) system for
trading/distribution businesses, with multi-tenant support (multiple
companies sharing the same application instance).

Set up a .NET 6 solution using Clean Architecture, named SmeErp, with
this exact project structure:

- SmeErp.Domain         - entities, enums, domain interfaces only, no dependencies
- SmeErp.Application    - DTOs, service interfaces, ServiceResult<T> pattern, business logic
- SmeErp.Infrastructure - EF Core DbContext, migrations, ASP.NET Identity, repository/service implementations
- SmeErp.Shared         - constants, setting keys, theme helper constants
- SmeErp.Web            - ASP.NET Core MVC (Razor views + controllers), wwwroot

Also create:
- SmeErp.Application.Tests — xUnit test project referencing Application

Architectural rules to follow throughout this project (fixed constraints,
not suggestions — apply them from this first step onward):

1. No business logic inside Razor views or controllers — controllers only
   orchestrate calls to Application-layer services.
2. No direct DbContext usage inside controllers — only inject services/repositories.
3. Application services return an explicit ServiceResult<T> wrapper for
   success/failure instead of throwing exceptions for expected validation errors.
4. Use SQL Server via EF Core 6 with Code-First migrations.
5. Every tenant-scoped entity (Product, Customer, Quotation, QuotationLine,
   CompanySetting, SigningKey excluded) carries a CompanyId foreign key to
   support multiple companies in one database — plan folder/namespace
   structure so a "current company" concept (e.g. an ICurrentCompanyService
   resolved per request) can be introduced cleanly later, without touching
   this scaffold again.
6. Company-level settings (including a primary theme color) will later
   drive both PDF branding and live UI theming — keep Shared open for a
   ThemeSettingsKeys-style constants file to be added in a later step.
7. Role-based access will be layered on top of ASP.NET Identity later,
   including a "CompetencyHead" role — don't implement roles yet, just
   don't preclude them.

For this first step only: scaffold the solution, project references
between layers, and empty folder structure per the layout above. Do not
implement any entities, migrations, DbContext, or features yet — those
come in separate, later prompts.

**Response summary:**
Cursor generated a .NET 6 Clean Architecture solution (SmeErp.sln) with
the following projects under src/: SmeErp.Domain, SmeErp.Application,
SmeErp.Infrastructure, SmeErp.Shared, SmeErp.Web, and
SmeErp.Application.Tests. Project references were wired between layers
per the requested architecture. No entities, DbContext, or features were
implemented at this stage, as instructed.

**Accepted / Changed / Rejected:**
- Accepted: overall project structure and layer separation as generated.
- Changed: added a .gitignore after noticing build output (bin/obj) was
  initially untracked/tracked incorrectly; excluded these going forward.
- Rejected: none at this stage — scaffold matched the request.

**Commit:** "Scaffold: Clean Architecture solution (prompt #1)"



## Prompt #2 — Domain Entities
**Date:** 2026-07-19

**Prompt:**
In the SmeErp.Domain project, create the following entities as plain
C# classes (no EF Core attributes yet — that comes later in Infrastructure
configuration):

Company
- Id (int)
- Name, Tagline, Address, City, State, Country, PinCode (string)
- GstNumber, PanNumber (string)
- Mobile, Email, Website (string)
- LogoPath (string, nullable)

CompanySetting
- Id (int)
- CompanyId (int, FK to Company)
- Key (string)
- Value (string)

Product
- Id (int)
- CompanyId (int, FK to Company)
- Name, Sku, Barcode (string)
- SellingPrice (decimal)
- GstPercent (decimal)
- CurrentStock (int)

Customer
- Id (int)
- CompanyId (int, FK to Company)
- Name, Code (string)
- Mobile, City, State, Address (string)

Quotation
- Id (int)
- CompanyId (int, FK to Company)
- QuotationNumber (string)
- CustomerId (int, FK to Customer)
- QuotationDate, ValidUntil (DateTime)
- SubTotal, TaxAmount, DiscountAmount, TotalAmount (decimal)
- Notes (string, nullable)

QuotationLine
- Id (int)
- QuotationId (int, FK to Quotation)
- ProductId (int, FK to Product)
- Quantity (int)
- UnitPrice (decimal)
- DiscountPercent (decimal)
- GstPercent (decimal)
- TaxAmount (decimal)
- TotalAmount (decimal)

Rules:
- Company itself does NOT have a CompanyId (it IS the tenant).
- Add simple navigation properties where natural (e.g. Quotation has a
  collection of QuotationLine, Product/Customer belong to Company) but
  do not add EF Core Fluent API configuration yet — that will be a
  separate step in the Infrastructure project.
- Do not create the DbContext, migrations, or seed data yet — entities only.
- Follow the existing project's namespace conventions (SmeErp.Domain.Entities).

**Response summary:**
Cursor generated 6 entities: Company (in SmeErp.Domain/Entities/) and
CompanySetting, Product, Customer, Quotation, QuotationLine (organized
under a new Entities/TenantScoped/ subfolder — Cursor's own addition,
not explicitly requested). CompanyId correctly omitted from Company and
present on all tenant-scoped entities. Navigation properties added
naturally (e.g. Quotation.Lines, Company.Products/Customers/Settings).

**Accepted / Changed / Rejected:**
- Accepted: all field names, types, and CompanyId placement exactly as specified.
- Accepted (unprompted): the TenantScoped/ subfolder split — a reasonable
  structural choice that visually reinforces the tenant boundary.
- Changed: none.
- Rejected: none.

**Commit:** "Add Domain entities (prompt #2)"


## Prompt #3 — DbContext, EF Core Configuration, Migration
**Date:** 2026-07-19

**Prompt:**
Now set up EF Core in SmeErp.Infrastructure for the entities already
created in SmeErp.Domain.

1. Create an AppDbContext class in SmeErp.Infrastructure/Persistence/
   that inherits from IdentityDbContext<ApplicationUser> (create a basic
   ApplicationUser class extending IdentityUser if it doesn't exist yet,
   with an additional FullName string property).

2. Add DbSet<T> properties for: Company, CompanySetting, Product,
   Customer, Quotation, QuotationLine.

3. In OnModelCreating, add Fluent API configuration for each entity:
   - Primary keys
   - Required string fields with reasonable max lengths (e.g. Name 200,
     Sku 100, Email 256, GstNumber 20)
   - Decimal precision or all money/percentage fields: use
     .HasPrecision(18, 2) for currency fields (SellingPrice, SubTotal,
     TaxAmount, DiscountAmount, TotalAmount, UnitPrice) and
     .HasPrecision(5, 2) for percentage fields (GstPercent, DiscountPercent)
   - Foreign key relationships with appropriate DeleteBehavior (use
     Restrict for Company -> Product/Customer/Quotation/CompanySetting
     to avoid cascade delete issues, Cascade for Quotation -> QuotationLine)
   - Index on CompanyId for every tenant-scoped entity, since these will
     be filtered on constantly

4. Register AppDbContext in SmeErp.Web's Program.cs (or Startup.cs,
   whichever this project uses) with SQL Server, reading the connection
   string from appsettings.json under a key called "DefaultConnection".
   Use a placeholder value like
   "Server=(localdb)\\mssqllocaldb;Database=SmeErpDb;Trusted_Connection=True;"
   in appsettings.json — no real credentials.

5. Generate the initial EF Core migration named "InitialCreate".

Do not implement seed data yet — that will be a separate prompt so I can
review the migration first. Do not implement authentication/login logic
yet either — just get AppDbContext, Identity wiring, and the migration
in place.

**Response summary:**
Cursor created AppDbContext (inheriting IdentityDbContext<ApplicationUser>)
in SmeErp.Infrastructure/Persistence/, added DbSets for all domain
entities, configured Fluent API (field lengths, decimal precision,
FK relationships, CompanyId indexes), registered EF Core + SQL Server
in SmeErp.Web's Program.cs reading from appsettings.json, and generated
the InitialCreate migration. Verified by running
`dotnet ef database update` against a local SQL Server Express instance
(DESKTOP-TUD67TP\SQLEXPRESS) — confirmed SmeErpDb was created with all
expected tables (Identity tables + Companies, Products, Customers,
Quotations, QuotationLines, CompanySettings).

**Accepted / Changed / Rejected:**
- Accepted: DbContext structure, Fluent API config, migration.
- Changed: updated appsettings.json connection string to point to local
  SQL Server Express instance instead of the LocalDB placeholder.
- Rejected: none.

**Commit:** "Add DbContext, EF Core config, and initial migration (prompt #3)"


## Prompt #4 — Seed Data
**Date:** 2026-07-19

**Prompt:**
Add seed data for the SmeErp application, applied via EF Core's
HasData() in OnModelCreating (not a runtime seeder for this data,
since it's fixed reference data, not something the app modifies).

Seed the following:

1. Two Company records (to demonstrate multi-tenant isolation later):
   - Company 1: "Sharma Trading Co." — a hardware/electricals distributor
     in Jaipur, Rajasthan, India, with realistic Indian GSTIN/PAN format
     values, address, mobile, email.
   - Company 2: "Verma Distributors" — a stationery/office-supplies
     distributor in Pune, Maharashtra, India, similarly realistic details.

2. For EACH company, seed:
   - 2 CompanySetting rows: one with Key "PrimaryColor" (a distinct hex
     value per company, e.g. one blue-ish, one green-ish), one with Key
     "InvoiceTerms" (a short realistic payment terms sentence).
   - 4 Products relevant to that company's business type, with realistic
     names, SKUs, barcodes (13-digit numeric), selling prices, a GST
     percent of either 5, 12, or 18, and a current stock quantity.
   - 3 Customers with realistic Indian business names, mobile numbers,
     cities matching or near the company's state, and addresses.

Use fixed, explicit integer IDs for all seeded rows (not auto-generated)
since HasData requires static seed values — start Company IDs at 1,
and keep Product/Customer IDs unique across companies (e.g. company 1's
products = 1-4, company 2's products = 5-8).

Do not seed Quotations or QuotationLines yet — those will come from
actual user interaction once the UI exists, not fixed seed data.

After adding the seed configuration, generate a new migration named
"SeedInitialData" and apply it.

**Response summary:**
Cursor created CompanyConfiguration.cs with HasData() seeding two
companies (Sharma Trading Co., Verma Distributors) with realistic
Indian business details. Generated and applied migration
20260719184645_SeedCompanies. However, the seed data for
CompanySetting, Product, and Customer was NOT included, despite being
part of the request — this was only caught by inspecting the database
directly in SSMS (see Prompt #5 for the correction).

**Accepted / Changed / Rejected:**
- Accepted: Company seed data — accurate and realistic.
- Changed: none.
- Rejected: none — the gap was incompleteness, not an incorrect
  response, addressed via a follow-up prompt rather than a rejection.

**Commit:** "Add seed data for companies (prompt #4)"


## Prompt #5 — Seed Data Correction (Products, Customers, CompanySettings)
**Date:** 2026-07-19

**Context:** Prompt #4 asked for seed data across Company, CompanySetting,
Product, and Customer, but verifying directly in SSMS showed only
Companies had been seeded. Confirmed via `grep -r "HasData"` that only
CompanyConfiguration.cs existed — Cursor had silently skipped the rest
of the original request.

**Prompt:**
The Company seed data was added correctly (see CompanyConfiguration.cs
and migration 20260719184645_SeedCompanies), but the seed data for
CompanySetting, Product, and Customer was NOT added, even though it was
requested. Please add it now:

1. Create a CompanySettingConfiguration.cs (or add to existing
   configuration classes, following the same pattern as
   CompanyConfiguration.cs) with HasData() seeding:
   - 2 CompanySetting rows for Company 1 (Sharma Trading Co., Id 1):
     Key "PrimaryColor" and Key "InvoiceTerms"
   - 2 CompanySetting rows for Company 2 (Verma Distributors, Id 2):
     same two keys, different values

2. Create/update ProductConfiguration.cs with HasData() seeding:
   - 4 Products for Company 1 (Ids 1-4), relevant to a hardware/electricals
     distributor
   - 4 Products for Company 2 (Ids 5-8), relevant to a stationery/office
     supplies distributor

3. Create/update CustomerConfiguration.cs with HasData() seeding:
   - 3 Customers for Company 1 (Ids 1-3)
   - 3 Customers for Company 2 (Ids 4-6)

Use realistic Indian business data as before. After this is added,
generate a new migration named "SeedProductsCustomersSettings" and
apply it — do not modify the existing SeedCompanies migration.

**Response summary:**
Cursor added CompanySettingConfiguration.cs, ProductConfiguration.cs,
and CustomerConfiguration.cs, each with HasData() seeding 2 settings,
4 products, and 3 customers per company respectively. Generated and
applied migration SeedProductsCustomersSettings. Verified in SSMS —
all four tables now show correct seeded rows matching each company's
business type.

**Accepted / Changed / Rejected:**
- Accepted: seed data across all three previously-missing entities.
- Changed: none.
- Rejected: none.

**Commit:** "Add missing seed data for products, customers, settings (prompt #5)"




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

## Prompt #7 — DB-Stored JWT Signing Key
**Date:** 2026-07-20

**Prompt:**
Add DB-stored JWT signing key support to the SmeErp application, so
JWT signing keys are generated at runtime and stored in the database
instead of being hardcoded in appsettings.json or the codebase.

1. Create a SigningKey entity in SmeErp.Domain:
   - Id (int)
   - KeyValue (string) — base64-encoded secret
   - CreatedAt (DateTime)
   - ExpiresAt (DateTime)
   - IsActive (bool)

2. Add a DbSet<SigningKey> to AppDbContext, with Fluent API config:
   required KeyValue with max length 500, an index on IsActive.

3. Create ISigningKeyService and SigningKeyService in
   SmeErp.Infrastructure:
   - GetActiveKeyAsync(): returns the current active, non-expired key,
     generating and persisting a new one via RandomNumberGenerator if
     none exists.
   - RotateKeyAsync(): deactivates existing active keys and generates
     a new one, with a 30-day expiration.

4. Register ISigningKeyService in DI as scoped.

5. Add a seeder (called at startup, alongside existing seeders) that
   ensures at least one active signing key exists on first run —
   do NOT hardcode any actual key value in code or configuration.

6. Generate a migration named "AddSigningKeyTable" and apply it.

Do not wire this key into actual JWT token generation/validation yet —
that's a separate step once we decide whether JWTs are used for a
separate API layer or purely for internal claims. For now, just get
the table, service, and seeding in place and confirmed working.

**Response summary:**
Cursor created a SigningKey entity, added it to AppDbContext with Fluent
API config (max length, IsActive index), implemented ISigningKeyService/
SigningKeyService with GetActiveKeyAsync() (generates via
RandomNumberGenerator if none exists) and RotateKeyAsync() (deactivates
old keys, creates new one with 30-day expiry), registered it in DI, and
added a startup seeder ensuring one active key exists on first run.
Generated migration 20260720054641_AddSigningKeyTable. Applying it
initially failed due to a stale dotnet process locking
SmeErp.Infrastructure.dll (see debugging-notes.md); resolved by killing
the process and rebuilding. Verified in SSMS via direct SQL query —
SigningKeys table contains exactly one active, non-hardcoded,
randomly-generated key with a correct 30-day expiration window.

**Accepted / Changed / Rejected:**
- Accepted: entity, service, seeding, migration — no key values hardcoded
  anywhere in source or config, satisfying the no-secrets-in-repo requirement.
- Changed: none in the generated code itself.
- Rejected: none.

**Commit:** "Add DB-stored JWT signing key generation and rotation (prompt #7)"


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


## Prompt #10 — Company Settings and Quotation PDF Generation
**Date:** 2026-07-20

**Prompt:**
Build the Company Settings page and Quotation PDF generation for the
SmeErp application, scoped to the current user's company.

1. Create a CompanySettingsViewModel/DTO covering: CompanyName, Address,
   City, State, Country, PinCode, GstNumber, PanNumber, Mobile, Email,
   Website, plus settings-driven values: PrimaryColor (hex), InvoiceTerms.

2. Create ICompanySettingsService/CompanySettingsService in
   Application/Infrastructure with:
   - GetAsync(companyId): reads the Company entity plus its
     CompanySetting rows (PrimaryColor, InvoiceTerms), returning a
     combined DTO. If PrimaryColor or InvoiceTerms settings don't exist
     yet for this company, return sensible defaults (e.g. "#1F2937" for
     color, a generic terms sentence) rather than failing.
   - UpdateAsync(companyId, dto): updates the Company entity's fields
     AND upserts the CompanySetting rows for PrimaryColor and
     InvoiceTerms (create if missing, update if present).

3. Create SettingsController ([Authorize]) with Index (GET, shows
   current settings in a form) and Index (POST, saves changes and
   redirects back with a success message).

4. Create Settings/Index.cshtml — a form covering all the fields above,
   including a simple HTML color picker input for PrimaryColor.

5. Implement PDF generation for a Quotation using QuestPDF (add the
   NuGet package; note it requires a community license configuration
   for non-commercial/small business use — set
   QuestPDF.Settings.License = LicenseType.Community in Program.cs).
   Create IQuotationPdfService/QuotationPdfService that, given a
   QuotationDetailDto and the company's CompanySettingsDto, generates a
   PDF with:
   - Header: company name, address, GSTIN, PAN, contact info, using
     PrimaryColor as an accent color for the header background/text
   - Quotation number, date, valid-until, customer name and address
   - A table of line items: product name, quantity, unit price,
     discount%, GST%, line total
   - Totals section: subtotal, discount, tax, grand total — also
     styled with PrimaryColor
   - Footer: invoice terms text from settings

6. Add a "Download PDF" button on the Quotation Details page/controller
   action that streams the generated PDF as a file download.

After implementing, this is important to verify manually: change the
Settings (address and PrimaryColor) and confirm the NEXT generated PDF
reflects the updated values — do not implement any caching that would
prevent this.

**Response summary:**
Cursor added CompanySettingsDto and ICompanySettingsService in
SmeErp.Application, with CompanySettingsService in Infrastructure
reading Company + CompanySetting rows (defaults "#1F2937" and generic
invoice terms when missing; upserts settings on update).
CompanySettingKeys constants were added to SmeErp.Shared. On the web
layer: CompanySettingsViewModel, SettingsController ([Authorize] GET/POST
Index with TempData success message), Settings/Index.cshtml with an HTML
color picker, and a Settings nav link for authenticated users. QuestPDF
2024.12.3 was added; QuestPDF.Settings.License = LicenseType.Community
was set in Program.cs. IQuotationPdfService/QuotationPdfService generates
a branded PDF (header with PrimaryColor accent, quotation meta, customer
address, line-items table, totals, footer terms). QuotationsController
DownloadPdf(id) loads fresh quotation and settings from the database on
each request (no caching) and streams the file; a Download PDF button
was added to Quotations/Details.cshtml. QuotationDetailDto was extended
with CustomerAddress, CustomerCity, and CustomerState for PDF display.
Settings-to-PDF consistency was manually verified by changing the company
address and PrimaryColor in Settings, then downloading a quotation PDF
and confirming the updated values appeared. A font rendering bug was found
after initial implementation: words containing "ti" were corrupted in the
PDF (e.g. "Quotation" rendered as "Quotaon", "Valid until" as "Valid unl")
due to QuestPDF's default font ligature handling. This was fixed in
QuotationPdfService by explicitly setting FontFamily("Arial") on the page
DefaultTextStyle and disabling StandardLigatures, rather than relying on
QuestPDF's automatic font fallback.

**Accepted / Changed / Rejected:**
- Accepted: company settings CRUD flow, QuestPDF integration, PDF download
  action, no caching on PDF generation, font rendering fix.
- Changed: QuotationDetailDto extended with customer address fields for
  PDF; DefaultTextStyle updated to use Arial with ligatures disabled after
  discovering the "ti" corruption bug.
- Rejected: none.

**Commit:** "Add company settings and quotation PDF generation (prompt #10)"
