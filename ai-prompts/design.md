# SmeErp Design Prompt Log

Copies of architecture and design decision prompts from `ai-prompts/planning.md`.
Original numbering preserved. See `planning.md` for the complete chronological log.

---

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
