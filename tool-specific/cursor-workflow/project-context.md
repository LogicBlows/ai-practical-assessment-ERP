# SmeErp Cursor Project Context

How architectural and coding context was established for Cursor during the SmeErp build, and why subsequent feature prompts did not need to re-state these constraints each time.

---

## Overview

This project does **not** use a `.cursor/rules` file or `.cursorrules` in the repository. Instead, project context was established in the **first Cursor prompt** (Prompt #1 — Solution Scaffold, recorded in `ai-prompts/planning.md`) and reinforced through:

1. The **generated solution structure** itself (layer names, project references, folder layout)
2. **Follow-on prompts** that explicitly scoped one feature at a time while saying "follow existing conventions"
3. The **accumulated codebase** — each merged PR gave Cursor more in-repo examples to match

The initial scaffold prompt served the **same purpose** a persistent `.cursor/rules` file would: it defined non-negotiable architectural rules once, at the start, so every later prompt could assume them.

---

## The Initial Scaffold Prompt (Prompt #1)

The first interaction established both **structure** and **rules**. The prompt asked Cursor to create a .NET 6 Clean Architecture solution with a fixed project layout:

| Project | Responsibility |
|---------|----------------|
| `SmeErp.Domain` | Entities, enums, domain interfaces — no external dependencies |
| `SmeErp.Application` | DTOs, service interfaces, `ServiceResult<T>`, business logic |
| `SmeErp.Infrastructure` | EF Core, migrations, Identity, service implementations |
| `SmeErp.Shared` | Constants, setting keys |
| `SmeErp.Web` | ASP.NET Core MVC (Razor + controllers) |
| `SmeErp.Application.Tests` | xUnit tests |

Critically, the prompt labeled seven items as **"Architectural rules to follow throughout this project (fixed constraints, not suggestions)"**:

### Rule 1 — No business logic in views or controllers

Controllers orchestrate Application-layer services only. Razor views display data and submit forms.

**How it persisted:** Every feature prompt (Products, Quotations, Settings, Search, Dashboard) specified "go through Application-layer services" and "[Authorize] on controllers." The codebase never introduced `DbContext` or calculation logic in controllers.

### Rule 2 — No direct `DbContext` in controllers

Only inject services/repositories from the Application/Infrastructure boundary.

**How it persisted:** All controllers inject `IProductService`, `IQuotationService`, `ICurrentCompanyService`, etc. Infrastructure services own EF queries.

### Rule 3 — `ServiceResult<T>` for expected failures

Application services return an explicit success/failure wrapper instead of throwing for validation errors.

**How it persisted:** Introduced in Prompt #8 and used consistently in `QuotationService`, `CompanySettingsService`, `SearchService`, `DashboardService`, and all controllers that check `result.Succeeded`.

### Rule 4 — SQL Server + EF Core 6 Code-First

Migrations live in Infrastructure; connection string in `appsettings.json`.

**How it persisted:** Every data-access feature used the existing `AppDbContext` pattern and Fluent API configurations.

### Rule 5 — Multi-tenant `CompanyId` convention

Tenant-scoped entities carry `CompanyId`. `Company` is the tenant root (no `CompanyId`). `SigningKey` was explicitly excluded from tenant scoping in the original rule. The prompt also reserved space for `ICurrentCompanyService` before it was built.

**How it persisted:** Prompt #6 added `ICurrentCompanyService` and `ApplicationUser.CompanyId` without restructuring. Every subsequent service method accepts `companyId` as an explicit parameter and filters queries accordingly.

### Rules 6–7 — Forward-looking constraints

Company settings would drive PDF and UI branding (`Shared` left open for constants). Role-based access would come later — don't preclude it.

**How it persisted:** `CompanySettingKeys` in Shared; `CompanyBrandingViewComponent` in Prompt #11; Identity roles seeded in Prompt #6 without role checks on controllers yet.

---

## Why Subsequent Prompts Didn't Re-Explain Constraints

After Prompt #1, feature prompts (Prompts #6–#14) followed a deliberate pattern:

| Technique | Example |
|-----------|---------|
| **Reference existing services** | "scoped to the current user's company via `ICurrentCompanyService`" |
| **Scope boundaries** | "Do not implement PDF yet — separate step" |
| **Point at conventions** | "following the same pattern as `CompanyConfiguration.cs`" |
| **Rely on generated code** | New controllers matched `ProductsController` / `QuotationsController` structure |

Cursor could read the repo — project references, `ServiceResult.cs`, existing controllers, and migration configurations — as living documentation. The scaffold prompt set the contract; the codebase enforced it on every new file.

This mirrors what a `.cursor/rules` file does: **write the rules once, apply everywhere**.

---

## Equivalent to `.cursor/rules`

A typical `.cursor/rules` or `AGENTS.md` might contain:

```markdown
- Clean Architecture: Domain → Application → Infrastructure → Web
- Controllers are thin; no DbContext in Web layer
- Services return ServiceResult<T>
- All tenant data filtered by CompanyId from ICurrentCompanyService
- EF Core 6, SQL Server, Code-First migrations
```

In this project, **Prompt #1 was that rules file**, except it was:

- Recorded in `ai-prompts/planning.md` (human-readable history)
- **Executed** as real solution structure (not just prose)
- **Verified** on every PR through manual review (`code-review-notes.md`)

No separate `.cursor/rules` was added because the constraints were embedded in the first prompt and the resulting codebase from day one.

---

## How Context Was Reinforced Over Time

| Phase | Context reinforcement |
|-------|----------------------|
| **Prompt #1** | Rules + empty layer structure |
| **Prompts #2–#5** | Entities, DbContext, seed data — patterns for Fluent API and `HasData()` |
| **Prompt #6** | `ICurrentCompanyService` fulfills Rule #5's reserved hook |
| **Prompts #8–#13** | Each feature adds a service interface + Infrastructure impl + thin controller — template established |
| **Prompt #14** | Tests extract pure logic (`QuotationTotalsCalculator`) per Rule #3 testability |
| **Code review** | Human review caught deviations (DbContext concurrency, missing DI) documented in `ai-prompts/code-review.md` |

Subsequent documentation prompts (README, design-notes, api-contract) were generated **from** the codebase, not as new rules — they describe what Prompt #1's constraints produced.

---

## Practical Guidance for Future Cursor Sessions

When continuing work on this repo in Cursor:

1. **Read Prompt #1** in `ai-prompts/planning.md` (or this file) before large new features.
2. **Follow existing service/controller pairs** — copy `QuotationsController` + `IQuotationService` pattern.
3. **Pass `companyId` explicitly** into every new Application service method.
4. **Return `ServiceResult<T>`** for business validation failures.
5. **Register new services** in `Program.cs` immediately (lesson from `IQuotationPdfService` gap).
6. **Never `Task.WhenAll` on one `DbContext`** (lesson from SearchService and DashboardService).

If adding a `.cursor/rules` file later, the seven rules from Prompt #1 plus the DbContext/DI lessons from code review would be the content to persist.

---

## Related Documentation

- [ai-prompts/planning.md](../../ai-prompts/planning.md) — Prompt #1 full text
- [ai-prompts/design.md](../../ai-prompts/design.md) — architecture prompt copies
- [design-notes.md](../../design-notes.md) — resulting architecture documentation
- [implementation-plan.md](../../implementation-plan.md) — AI usage plan across 14 prompts
- [code-review-notes.md](../../code-review-notes.md) — where rules were enforced in review
