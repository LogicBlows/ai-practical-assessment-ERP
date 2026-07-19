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