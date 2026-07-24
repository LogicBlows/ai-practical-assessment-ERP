# SmeErp

## Project Overview

SmeErp is a small business ERP web application for trading and distribution companies. It supports multiple companies (tenants) in a single database, with per-company products, customers, and quotations. Users sign in with ASP.NET Identity, manage company settings and branding, create and view quotations (including PDF download), search across products/customers/quotations, and view dashboard KPIs — all scoped to their company.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 6 |
| ORM | Entity Framework Core 6 (SQL Server) |
| Web UI | ASP.NET Core MVC (Razor views) |
| Authentication | ASP.NET Identity |
| PDF generation | QuestPDF 2024.12.3 |
| Tests | xUnit, EF Core InMemory provider |

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- SQL Server (Express or a full instance)
- [EF Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (`dotnet-ef`):

  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Setup

### 1. Clone the repository

```bash
git clone <repository-url>
cd ai-Practical-Assessment
```

### 2. Configure the database connection string

Edit `src/SmeErp.Web/appsettings.json` and set `ConnectionStrings:DefaultConnection` to point at your SQL Server instance. Use a placeholder format like this (replace `YOUR_SERVER` with your server name):

```json
"DefaultConnection": "Server=YOUR_SERVER;Database=SmeErpDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Do not commit real credentials. For a named instance, use `Server=YOUR_SERVER\\SQLEXPRESS;...`.

### 3. Apply database migrations

From the repository root:

```bash
dotnet ef database update --project src/SmeErp.Infrastructure --startup-project src/SmeErp.Web
```

This creates the `SmeErpDb` database and applies these migrations:

| Migration | Purpose |
|-----------|---------|
| `InitialCreate` | Schema (entities + ASP.NET Identity tables) |
| `SeedCompanies` | Two demo companies (Sharma Trading Co., Verma Distributors) |
| `SeedProductsCustomersSettings` | Products, customers, and company settings per tenant |
| `SeedUsersAndRoles` | Identity role seed data |
| `AddSigningKeyTable` | Signing key storage table |

On first run, the app also seeds demo users/roles (`IdentitySeeder`) and ensures an active signing key exists (`SigningKeySeeder`).

### 4. Run the application

```bash
cd src/SmeErp.Web
dotnet run
```

### 5. Open the app

Navigate to the URL printed in the console. By default (from `launchSettings.json`):

- HTTPS: `https://localhost:7211`
- HTTP: `http://localhost:5057`

You will be redirected to the login page at `/Account/Login`.

## Demo Login Credentials

> **Local/demo use only.** These passwords are hardcoded placeholders and must not be used in production.

| Company | Email | Password | Role |
|---------|-------|----------|------|
| Sharma Trading Co. | `admin@sharmatrading.com` | `Passw0rd!123` | Proprietor |
| Verma Distributors | `admin@vermadist.com` | `Passw0rd!123` | Proprietor |

Each user only sees data for their own company (products, customers, quotations, settings, search results, and dashboard counts).

## Running Tests

From the repository root:

```bash
dotnet test
```

The test project (`tests/SmeErp.Application.Tests`) includes two xUnit tests:

1. **Quotation calculation** — verifies line-item and quotation total math (`QuotationTotalsCalculator`).
2. **Settings defaults** — verifies default `PrimaryColor` and `InvoiceTerms` when no company settings exist.

## Project Structure

Clean Architecture layout under `src/`:

```
src/
├── SmeErp.Domain/          # Entities and domain types (no external dependencies)
│   └── Entities/           # Company, Product, Customer, Quotation, SigningKey, etc.
├── SmeErp.Application/     # DTOs, service interfaces, ServiceResult<T>, business logic
│   ├── Common/             # ServiceResult wrapper
│   ├── DTOs/               # Data transfer objects
│   ├── Interfaces/         # Service contracts (IProductService, IQuotationService, …)
│   └── Services/           # Pure calculation helpers (e.g. QuotationTotalsCalculator)
├── SmeErp.Infrastructure/  # EF Core DbContext, migrations, Identity, service implementations
│   ├── Identity/           # ApplicationUser, IdentitySeeder
│   ├── Persistence/        # AppDbContext, Fluent API configs, migrations, HasData seed
│   └── Services/           # ProductService, QuotationService, SearchService, etc.
├── SmeErp.Shared/          # Cross-cutting constants (e.g. CompanySettingKeys)
└── SmeErp.Web/             # ASP.NET Core MVC host
    ├── Controllers/        # MVC controllers (orchestrate Application services)
    ├── Models/             # View models
    ├── Views/              # Razor views
    └── wwwroot/            # Static assets (CSS, JS)
```

`tests/SmeErp.Application.Tests/` — xUnit tests referencing Application and Infrastructure.

Controllers do not contain business logic or direct `DbContext` usage; they call Application-layer services that return `ServiceResult<T>`.

## Key Features

- **Multi-tenant isolation** — tenant-scoped entities carry a `CompanyId`; `ICurrentCompanyService` resolves the logged-in user's company and all queries are filtered accordingly.
- **Products & customers** — company-scoped list pages with keyword search (name, SKU/barcode for products; name/code for customers).
- **Quotations** — create, list, and view quotations with per-line discount/GST calculations and auto-generated quotation numbers (`QT-{CompanyId}-{sequential}`).
- **Quotation PDF generation** — download branded PDFs via QuestPDF, using company settings (name, address, GSTIN, primary color, invoice terms).
- **Company settings** — edit company profile and settings (`PrimaryColor`, `InvoiceTerms`); primary color drives navbar branding and PDF accent color.
- **Global search** — search products, customers, and quotations (minimum 2 characters) from the navigation bar.
- **Dashboard KPIs** — total products, total customers, quotations created today, and pending (still-valid) quotations.
