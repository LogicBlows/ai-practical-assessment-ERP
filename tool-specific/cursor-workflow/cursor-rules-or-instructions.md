# SmeErp Cursor Rules / Instructions

Persistent architectural rules given to Cursor in **Prompt #1** (Solution Scaffold, 2026-07-19) and applied throughout all 14 implementation prompts. These functioned as the project's standing instructions — equivalent to a `.cursor/rules` file — even though no separate rules file was committed to the repository.

Source: [ai-prompts/planning.md](../../ai-prompts/planning.md) (Prompt #1); indexing strategy from Prompt #3 (DbContext setup).

---

## Purpose

When building SmeErp, the first prompt established **fixed constraints, not suggestions**. Every subsequent feature prompt (auth, products, quotations, PDF, search, dashboard, tests) assumed these rules without re-stating them. Cursor matched existing service/controller patterns in the accumulated codebase.

---

## Solution Structure (Clean Architecture)

Create and maintain a .NET 6 solution named **SmeErp** with this exact project layout:

| Project | Responsibility |
|---------|----------------|
| `SmeErp.Domain` | Entities, enums, domain interfaces only — **no dependencies** |
| `SmeErp.Application` | DTOs, service interfaces, `ServiceResult<T>` pattern, business logic |
| `SmeErp.Infrastructure` | EF Core `DbContext`, migrations, ASP.NET Identity, repository/service implementations |
| `SmeErp.Shared` | Constants, setting keys, theme helper constants |
| `SmeErp.Web` | ASP.NET Core MVC (Razor views + controllers), `wwwroot` |
| `SmeErp.Application.Tests` | xUnit test project referencing Application |

**Dependency direction:** Domain ← Application ← Infrastructure ← Web. Web references Application and Infrastructure for DI registration only; controllers never reference `DbContext` directly.

**Namespace convention:** `SmeErp.Domain.Entities` for entities; tenant-scoped types under `Entities/TenantScoped/` (adopted in Prompt #2).

---

## Architectural Rules (Prompt #1)

> *Architectural rules to follow throughout this project (fixed constraints, not suggestions — apply them from this first step onward):*

### Rule 1 — No business logic in Razor views or controllers

Controllers **only orchestrate** calls to Application-layer services. Razor views display data and submit forms — no calculations, validation logic, or database access in views.

**In practice:**
- Quotation totals live in `QuotationService` / `QuotationTotalsCalculator`, not in `Create.cshtml`.
- Controllers map `ServiceResult<T>` to views, `ModelState`, redirects, or HTTP status codes.

### Rule 2 — No direct `DbContext` in controllers

Controllers inject **services and repositories** from the Application/Infrastructure boundary only. All EF Core queries belong in Infrastructure service implementations.

**In practice:**
- `ProductsController` injects `IProductService`, not `AppDbContext`.
- `Program.cs` registers services; every new interface gets a DI registration immediately.

### Rule 3 — `ServiceResult<T>` for expected failures

Application services return an explicit **`ServiceResult<T>`** (or `ServiceResult`) wrapper for success/failure instead of throwing exceptions for expected validation errors.

**In practice:**
```csharp
// Application layer — return failure, don't throw
public static ServiceResult<T> Failure(string error) =>
    new() { Succeeded = false, Error = error };

// Controller — check result
if (!result.Succeeded)
{
    ModelState.AddModelError(string.Empty, result.Error!);
    return View(model);
}
```

Used in: `QuotationService`, `CompanySettingsService`, `SearchService`, `DashboardService`, and all controllers that handle writes or guarded reads.

### Rule 4 — SQL Server + EF Core 6 Code-First

- Migrations live in **Infrastructure** (`SmeErp.Infrastructure/Persistence/Migrations/`).
- Connection string key: **`DefaultConnection`** in `appsettings.json`.
- Use a placeholder connection string (no real credentials in source).
- Fluent API configuration in `Persistence/Configurations/` — no EF attributes on Domain entities.

### Rule 5 — Multi-tenant `CompanyId` convention

Every **tenant-scoped entity** carries a `CompanyId` foreign key to support multiple companies in one database:

| Entity | `CompanyId` |
|--------|-------------|
| `Product` | Yes |
| `Customer` | Yes |
| `Quotation` | Yes |
| `QuotationLine` | No (scoped via parent `Quotation`) |
| `CompanySetting` | Yes |
| `Company` | **No** — it is the tenant root |
| `SigningKey` | **No** — global, not per-tenant (excluded in Prompt #1) |

**Current company resolution:**
- `ICurrentCompanyService` resolves the logged-in user's `CompanyId` per request (added Prompt #6).
- Every Application service method accepts **`companyId` as an explicit parameter**.
- All queries filter by `CompanyId`; cross-tenant access returns `ServiceResult.Failure` or `404`.

**Folder structure:** Plan namespaces so `ICurrentCompanyService` could be introduced later without restructuring the scaffold.

### Rule 6 — Company settings drive branding (forward-looking)

Company-level settings (including a **primary theme color**) will drive both **PDF branding** and **live UI theming**. Keep `SmeErp.Shared` open for a `ThemeSettingsKeys`-style constants file.

**How it was fulfilled:**
- `CompanySettingKeys` in Shared.
- `PrimaryColor` and `InvoiceTerms` in settings → navbar via `CompanyBrandingViewComponent` (Prompt #11) and PDF accent via `QuotationPdfService` (Prompt #10).

### Rule 7 — Role-based access (forward-looking)

Role-based access will be layered on top of ASP.NET Identity later, including a **"CompetencyHead"** role. Do not implement roles in the scaffold step — just don't preclude them.

**How it was fulfilled:**
- Identity + roles (`Admin`, `Proprietor`) seeded in Prompt #6.
- Controllers use `[Authorize]` only; no `[Authorize(Roles = "...")]` enforcement in Core scope.
- `CompetencyHead` mentioned in Prompt #1 but not seeded.

---

## Indexing Strategy (Prompt #3)

Established when `AppDbContext` and Fluent API configuration were added. This is a **persistent data-layer rule** alongside the Prompt #1 constraints.

### `CompanyId` indexes on tenant-scoped entities

> *Index on `CompanyId` for every tenant-scoped entity, since these will be filtered on constantly.*

Applied in every `*Configuration.cs` for entities that carry `CompanyId`:

```csharp
builder.HasIndex(p => p.CompanyId);
```

**Indexed entities:** `Product`, `Customer`, `Quotation`, `CompanySetting`, `ApplicationUser` (links user to company).

### Foreign key indexes

Additional indexes on FK columns used in joins and lookups:

| Entity | Index |
|--------|-------|
| `Quotation` | `CompanyId`, `CustomerId` |
| `QuotationLine` | `QuotationId`, `ProductId` |
| `SigningKey` | `IsActive` (active key lookup) |

### Delete behavior

| Relationship | `DeleteBehavior` |
|--------------|------------------|
| `Company` → `Product` / `Customer` / `Quotation` / `CompanySetting` | `Restrict` (avoid cascade delete) |
| `Quotation` → `QuotationLine` | `Cascade` |

### Decimal precision

| Field type | Precision |
|------------|-----------|
| Currency (`SellingPrice`, `SubTotal`, `TaxAmount`, `UnitPrice`, etc.) | `(18, 2)` |
| Percentages (`GstPercent`, `DiscountPercent`) | `(5, 2)` |

---

## Rules Reinforced During Development

These were not in Prompt #1 but were enforced through code review and became standing practice:

| Lesson | Rule |
|--------|------|
| DbContext concurrency | **Never** `Task.WhenAll` on parallel queries sharing one scoped `DbContext` — run sequentially or use separate scopes. |
| DI registration | Register every new service interface in `Program.cs` when the implementation is added (e.g. `IQuotationPdfService` gap). |
| Testability | Extract pure calculation logic (`QuotationTotalsCalculator`) so Application.Tests can cover it without a database. |

Documented in: [ai-prompts/code-review.md](../../ai-prompts/code-review.md), [code-review-notes.md](../../code-review-notes.md).

---

## Copy-Paste Summary (`.cursor/rules` equivalent)

```markdown
# SmeErp — standing rules (from Prompt #1)

- Clean Architecture: Domain → Application → Infrastructure → Web
- Controllers are thin: orchestrate Application services only
- No DbContext, business logic, or EF queries in Web layer or Razor views
- Application services return ServiceResult<T> for validation/business failures
- SQL Server, EF Core 6 Code-First; migrations in Infrastructure
- Multi-tenant: filter all tenant data by CompanyId from ICurrentCompanyService
- Company is tenant root (no CompanyId); SigningKey is global (no CompanyId)
- Index CompanyId on every tenant-scoped entity; Restrict delete on Company FKs
- Company settings (PrimaryColor, InvoiceTerms) drive UI and PDF branding
- Identity roles exist; [Authorize] required on business controllers
```

---

## Related Documentation

- [project-context.md](project-context.md) — how these rules persisted without a `.cursor/rules` file
- [spec.md](spec.md) — functional requirements built under these rules
- [tasks.md](tasks.md) — build sequence (Prompts #1–#14)
- [design-notes.md](../../design-notes.md) — resulting architecture documentation
- [ai-prompts/planning.md](../../ai-prompts/planning.md) — full Prompt #1 text and response
