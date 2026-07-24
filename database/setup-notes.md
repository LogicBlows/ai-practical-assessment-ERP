# SmeErp Database Setup Notes

Database configuration, migrations, seed data, and persistence verification for local development.

---

## Database Choice

SmeErp uses **Microsoft SQL Server** as the persistence store, accessed via **Entity Framework Core 6** with a **Code-First** approach.

| Aspect | Detail |
|--------|--------|
| ORM | EF Core 6 (`Microsoft.EntityFrameworkCore.SqlServer` 6.0.36) |
| DbContext | `AppDbContext` in `src/SmeErp.Infrastructure/Persistence/` (extends `IdentityDbContext<ApplicationUser>`) |
| Migrations | `src/SmeErp.Infrastructure/Persistence/Migrations/` |
| Database name | `SmeErpDb` (default in connection string) |

Schema is created and updated through EF migrations — not hand-written SQL scripts.

---

## Connection String

The connection string is read from `src/SmeErp.Web/appsettings.json` under `ConnectionStrings:DefaultConnection`.

**Placeholder format** (replace `YOUR_SERVER` with your SQL Server instance name):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SmeErpDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

For a named instance (e.g. SQL Server Express):

```json
"DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS;Database=SmeErpDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

**Notes:**

- Uses **Windows authentication** (`Trusted_Connection=True`) — no SQL username/password in the file.
- `TrustServerCertificate=True` is typical for local development with self-signed certificates.
- The checked-in `appsettings.json` contains a **developer machine-specific** server name (`DESKTOP-TUD67TP\SQLEXPRESS`). Replace this with your own instance before running locally.
- **Do not commit real credentials** (SQL logins, passwords, production connection strings).

`appsettings.Development.json` does not override the connection string — all configuration is in `appsettings.json`.

---

## Apply Migrations

From the **repository root**, run:

```bash
dotnet ef database update --project src/SmeErp.Infrastructure --startup-project src/SmeErp.Web
```

**Prerequisite:** Install the EF Core CLI tool if not already present:

```bash
dotnet tool install --global dotnet-ef
```

This command:

1. Creates the `SmeErpDb` database if it does not exist
2. Applies all pending migrations in order

### Migrations applied

| Migration | What it does |
|-----------|--------------|
| `20260719123024_InitialCreate` | Schema: domain entities + ASP.NET Identity tables |
| `20260719184645_SeedCompanies` | Seeds 2 companies via `HasData()` |
| `20260719185511_SeedProductsCustomersSettings` | Seeds products, customers, and company settings per tenant |
| `20260719192330_SeedUsersAndRoles` | Identity role seed data (migration scaffolding) |
| `20260720054641_AddSigningKeyTable` | `SigningKeys` table for runtime-generated signing keys |

### Runtime seeders (on application startup)

After migrations, the app runs seeders in `Program.cs`:

| Seeder | Purpose |
|--------|---------|
| `IdentitySeeder` | Creates roles and demo users if they do not exist |
| `SigningKeySeeder` | Ensures at least one active signing key exists (generated at runtime, not hardcoded) |

---

## Seed Data

### Companies (migration — `HasData()`)

| Id | Name | Business type | Location |
|----|------|---------------|----------|
| 1 | Sharma Trading Co. | Hardware & electricals distributor | Jaipur, Rajasthan |
| 2 | Verma Distributors | Stationery & office supplies | Pune, Maharashtra |

### Company settings (migration — per company)

Each company has two `CompanySetting` rows:

| Key | Company 1 (Sharma) | Company 2 (Verma) |
|-----|--------------------|-------------------|
| `PrimaryColor` | `#1E40AF` (blue) | `#047857` (green) |
| `InvoiceTerms` | 15-day payment terms (Jaipur-style) | 30-day payment terms (Pune jurisdiction) |

### Products (migration — 4 per company)

| Company | Product IDs | Examples |
|---------|-------------|----------|
| Sharma Trading Co. | 1–4 | Hardware/electricals items |
| Verma Distributors | 5–8 | Stationery/office supplies |

Each product has name, SKU, barcode, selling price, GST %, and current stock.

### Customers (migration — 3 per company)

| Company | Customer IDs | Location focus |
|---------|--------------|----------------|
| Sharma Trading Co. | 1–3 | Jaipur / Rajasthan area |
| Verma Distributors | 4–6 | Pune / Maharashtra area |

### Roles and users

**Roles** (seeded at startup via `IdentitySeeder`):

- `Admin`
- `Proprietor`

**Users** (seeded at startup; local/demo only):

| Email | Password | Company | Role |
|-------|----------|---------|------|
| `admin@sharmatrading.com` | `Passw0rd!123` | Sharma Trading Co. (Id 1) | Proprietor |
| `admin@vermadist.com` | `Passw0rd!123` | Verma Distributors (Id 2) | Proprietor |

Password is defined in `IdentitySeeder.cs` with a comment marking it as local/demo use only. Users are skipped if they already exist (idempotent on restart).

**Quotations** are not seeded — they are created through the UI after login.

### Signing keys

Not seeded via migration. `SigningKeySeeder` calls `ISigningKeyService.GetActiveKeyAsync()` on first run to generate and persist a random key in the `SigningKeys` table.

---

## Verify Persistence After Restart

Data persists in SQL Server across application restarts. This was **manually verified during development** using both the UI and SQL Server Management Studio (SSMS).

### UI verification

1. Apply migrations and run the app (`dotnet run` from `src/SmeErp.Web`).
2. Log in as `admin@sharmatrading.com` / `Passw0rd!123`.
3. Confirm seeded data: 4 products, 3 customers on list pages; dashboard KPI counts.
4. Create a quotation and note the quotation number.
5. **Stop the application** (Ctrl+C or stop the `dotnet` process).
6. **Start the app again** (`dotnet run`).
7. Log in with the same credentials.
8. Confirm:
   - Products and customers lists still show seeded data
   - The quotation created in step 4 still appears in the list and opens on the detail page
   - Dashboard counts reflect persisted quotations

Repeat with `admin@vermadist.com` to confirm tenant isolation (different products/customers; no access to Sharma's quotations).

### SSMS verification

1. Connect to the SQL Server instance used in `appsettings.json`.
2. Open database `SmeErpDb`.
3. Inspect tables after migration:

   ```sql
   SELECT COUNT(*) FROM Companies;           -- expect 2
   SELECT COUNT(*) FROM Products;            -- expect 8 (4 per company)
   SELECT COUNT(*) FROM Customers;         -- expect 6 (3 per company)
   SELECT COUNT(*) FROM CompanySettings;     -- expect 4 (2 per company)
   SELECT COUNT(*) FROM AspNetUsers;         -- expect 2 after first app run
   SELECT COUNT(*) FROM SigningKeys;         -- expect 1+ after first app run
   ```

4. After creating a quotation in the UI, confirm a row exists in `Quotations` and related rows in `QuotationLines`.
5. Stop and restart the app — re-query the same tables; row counts and quotation data should be unchanged.

### What persistence means in this project

- **Migration seed data** (`HasData()`) is written once when migrations are applied and survives restarts.
- **User-created data** (quotations, settings updates) is saved via `SaveChangesAsync()` to SQL Server and survives restarts.
- **Runtime seeders** (`IdentitySeeder`, `SigningKeySeeder`) are idempotent — they do not duplicate users or keys on subsequent startups.

---

## Troubleshooting

| Issue | Action |
|-------|--------|
| Migration fails — cannot connect | Verify SQL Server is running; check `DefaultConnection` server name in `appsettings.json` |
| Build fails — DLL locked | Kill stale `dotnet run` processes before rebuild (`debugging-notes.md` Issues 1–2) |
| Empty Products/Customers tables after migrate | Ensure migration `SeedProductsCustomersSettings` was applied (not just `SeedCompanies`) |
| Login fails after fresh migrate | Run the app once so `IdentitySeeder` creates users, or check `AspNetUsers` in SSMS |

---

## Related Documentation

- [README.md](../README.md) — full setup instructions
- [data-model.md](../data-model.md) — entity model and relationships
- [ai-prompts/planning.md](../ai-prompts/planning.md) — Prompts #3–#5 (DbContext, seed data)
