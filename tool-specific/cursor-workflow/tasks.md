# SmeErp Task Sequence

Actual build order executed for Option 3 — .NET Full-Stack SME ERP. Same content as the **Task Breakdown** section in [implementation-plan.md](../../implementation-plan.md), reformatted as a numbered checklist.

**Status:** All 14 implementation tasks complete.

---

## Implementation Tasks

1. **Done** — **Solution scaffold**  
   .NET 6 Clean Architecture solution: Domain, Application, Infrastructure, Shared, Web, Application.Tests; project references wired.  
   `f107a08`

2. **Done** — **Domain entities**  
   `Company`, `CompanySetting`, `Product`, `Customer`, `Quotation`, `QuotationLine` entity classes.  
   `1b475da`

3. **Done** — **DbContext / migrations**  
   `AppDbContext`, Fluent API config, `ApplicationUser`, `InitialCreate` migration, SQL Server registration.  
   `8e2935f`

4. **Done** — **Seed data (companies)**  
   Two demo companies via `HasData()`; `SeedCompanies` migration.  
   `f539c65`

5. **Done** — **Seed data correction**  
   Products, customers, company settings per company; `SeedProductsCustomersSettings` migration (follow-up after incomplete Prompt #4).  
   `226516f`

6. **Done** — **Authentication / roles / current company**  
   ASP.NET Identity, seeded roles (`Admin`, `Proprietor`) and users, login/logout, `ICurrentCompanyService`, `[Authorize]` dashboard placeholder.  
   `e73ccec` → PR #1 `feature/auth-and-roles`

7. **Done** — **DB-stored JWT signing key**  
   `SigningKey` entity, `ISigningKeyService`, runtime key generation/rotation, `AddSigningKeyTable` migration, startup seeder.  
   `d46e8eb` (same PR #1)

8. **Done** — **Products / customers list + search**  
   `IProductService`, `ICustomerService`, controllers, Razor Index views, nav links.  
   `e7b2fab` → PR #2 `feature/products-customers`

9. **Done** — **Quotation create / list / detail**  
   `IQuotationService`, line/total calculations, create form with repeatable lines, Index and Details views.  
   `d799cf2` → PR #3 `feature/quotations`

10. **Done** — **Company settings + PDF**  
    `ICompanySettingsService`, Settings page, QuestPDF integration, PDF download; font ligature bug fixed.  
    `8e2f3b5` → PR #4 `feature/settings-and-pdf`

11. **Done** — **UI styling pass**  
    Bootswatch Flatly CDN, `CompanyBrandingViewComponent`, card layouts, striped tables.  
    `5425453` → PR #5 `feature/ui-styling`

12. **Done** — **Global search**  
    `ISearchService`, `SearchController`, navbar search form, grouped results view; DbContext concurrency fix.  
    `7c20d79` (partial) → PR #6 `feature/search-and-dashboard`

13. **Done** — **Dashboard KPIs**  
    `IDashboardService`, four KPI cards; DbContext concurrency fix (recurring pattern).  
    `7c20d79` → PR #6

14. **Done** — **Mandatory xUnit tests**  
    `QuotationTotalsCalculator` extraction, quotation calculation test, settings defaults test.  
    `767cbdc` → PR #7 `feature/tests`

---

## Post-Core Documentation

Documentation written after PR #7 (on `main`):

| # | Document | Commit |
|---|----------|--------|
| 1 | `README.md` | `a0b6c69` |
| 2 | `data-model.md` | `eab0c15` |
| 3 | `api-contract.md` | `93cf6dc` |
| 4 | `design-notes.md` | `0359a09` |
| 5 | `ui-flow.md` | `9dc1b3a` |
| 6 | `acceptance-criteria.md` | `cbc7830` |
| 7 | `requirements-analysis.md` | `3609769` |

---

## Related Documentation

- [implementation-plan.md](../../implementation-plan.md) — full plan with milestones and risks
- [spec.md](spec.md) — functional spec (Core + Stretch adopted)
- [project-context.md](project-context.md) — Cursor context and architectural rules
- [ai-prompts/planning.md](../../ai-prompts/planning.md) — original prompt log (Prompts #1–#14)
