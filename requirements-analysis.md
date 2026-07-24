# SmeErp Requirements Analysis

**Project:** Option 3 — .NET Full-Stack SME ERP (Inventory Management)  
**Stack:** .NET 6, ASP.NET Core MVC, EF Core 6, SQL Server, ASP.NET Identity, QuestPDF

---

## Selected Project Option

**.NET Full-Stack — SME ERP (Inventory Management)**

A multi-tenant web application for small trading and distribution businesses to manage products, customers, and sales quotations — including PDF generation, company branding via settings, global search, and a dashboard of key metrics. Built as a Clean Architecture .NET solution with Razor MVC for the UI and SQL Server for persistence.

---

## My Understanding

This project is a small ERP system for trading/distribution businesses, built to demonstrate multi-tenant SaaS-style architecture - a single application instance serving multiple independent businesses, each with fully isolated data. The core workflow centers on quotation generation: staff select a customer, add line items from the product catalog, and the system calculates tax and totals automatically before producing a branded PDF. I chose to extend the seeded multi-tenant setup with an additional role (originally requested by my supervisor for a separate initiative) to also demonstrate DB-stored JWT signing key management, since that reflected a real gap I found in another codebase I work on.

---

## Functional Requirements

The following functional requirements are **implemented** in the current codebase:

### Authentication and session management

- Users sign in with email and password via ASP.NET Identity (cookie-based sessions).
- Seeded demo accounts exist for two companies; there is no self-registration flow.
- Unauthenticated users are redirected to `/Account/Login` when accessing protected pages.
- Users can sign out, which clears the session and returns them to the login page.

### Multi-tenant company isolation

- Multiple companies share one database; each company's data is isolated by `CompanyId`.
- The logged-in user's company is resolved per request via `ICurrentCompanyService`.
- All business queries (products, customers, quotations, search, dashboard, settings) are scoped to the current user's company.

### Product management (list and search)

- Authenticated users can view a company-scoped list of products.
- Users can search products by name, SKU, or barcode (GET query parameter).
- List-only — no create, edit, or delete UI for products.

### Customer management (list and search)

- Authenticated users can view a company-scoped list of customers.
- Users can search customers by name or code.
- List-only — no create, edit, or delete UI for customers.

### Quotation creation with line-item calculation

- Users can create a quotation via a form: select customer, set dates and optional notes, add multiple line items (product, quantity, unit price, discount %).
- Per-line calculations: subtotal, discount, GST (from product record), tax, and line total.
- Quotation-level totals: subtotal, total discount, total tax, grand total.
- Auto-generated quotation number format: `QT-{CompanyId}-{sequential}`.
- Create-only — quotations cannot be edited after creation.

### Quotation list and detail view

- Users can view a list of their company's quotations (newest first).
- Users can open a read-only detail view showing header, line items, and totals.

### Quotation PDF generation

- Users can download a branded PDF from the quotation detail page.
- PDF includes company header (name, address, GSTIN, PAN, contact), quotation metadata, customer info, line-item table, totals, and invoice terms footer.
- Accent color in the PDF comes from company settings.

### Company settings management

- Users can view and update their company's profile (name, address, contact, GST, PAN, etc.).
- Users can set `PrimaryColor` (drives navbar branding and PDF accent) and `InvoiceTerms` (PDF footer).
- Settings are persisted to the database and loaded fresh on each PDF generation.

### Global search

- Authenticated users can search across products, customers, and quotations from the navbar (minimum 2 characters).
- Results are grouped by type with links to the relevant page.

### Dashboard KPIs

- Dashboard displays four company-scoped counts: total products, total customers, quotations created today, and pending quotations (valid until ≥ today).

---

## Non-Functional Requirements

Based on what was actually built and verified:

| Requirement | How it is met |
|-------------|---------------|
| **Data isolation between tenants** | Every tenant-scoped query filters by `CompanyId`; cross-tenant access returns failure or `404`. Manually verified with both seeded companies. |
| **No secrets in source control** | Signing keys generated at runtime and stored in the database; connection string uses Windows authentication placeholder; demo password explicitly marked local/demo-only in `IdentitySeeder`. |
| **Responsive validation** | Invalid form input and business-rule failures surface via `ModelState` errors on POST actions; services return `ServiceResult.Failure` with readable messages. |
| **Calculation correctness** | Quotation totals verified by hand-calculation against UI output (Prompt #9); `QuotationTotalsCalculator` covered by mandatory xUnit test (2/2 passing). |
| **Separation of concerns** | Clean Architecture layers; no business logic in Razor views; no `DbContext` in controllers. |
| **Persistence** | SQL Server via EF Core Code-First migrations; data survives application restarts. |
| **Usability** | Bootswatch-themed responsive Bootstrap UI; card layouts; striped tables; settings-driven navbar branding. |

---

## Assumptions

Assumptions **actually made** during development (reflected in the codebase):

| Assumption | Rationale / evidence |
|------------|---------------------|
| **One seeded user per company is sufficient for Core scope** | Two companies, two users (`admin@sharmatrading.com`, `admin@vermadist.com`). No user-management UI. Schema supports multiple users per company (`ApplicationUser.CompanyId`) but this was not built out. |
| **No self-service tenant registration** | Companies and users are seeded by the developer (`HasData()` migrations + `IdentitySeeder` at startup). No registration or onboarding flow. |
| **Single role tier is sufficient for Core** | `Admin` and `Proprietor` roles are seeded; both demo users are assigned `Proprietor` only. Controllers use `[Authorize]` without role checks — any authenticated user has full access to their company's data. |
| **Products and customers are reference data** | Seeded via migrations; list/search only. No CRUD forms for catalog management in Core scope. |
| **Quotations are immutable after creation** | No edit or delete actions; only create, list, view, and PDF download. |
| **GST rate comes from the product** | Users set unit price and discount on quotation lines; `GstPercent` is read from the product at creation time, not entered on the form. |
| **Shared database multi-tenancy** | All companies in one SQL Server database, isolated by `CompanyId` column — not separate databases per tenant. |
| **Cookie authentication is sufficient** | ASP.NET Identity with application cookies; JWT signing key infrastructure exists but is not wired to token auth yet. |
| **English-only UI** | All labels, messages, and seeded data are in English. |
| **Local/demo credentials are acceptable** | Placeholder password in `IdentitySeeder` with an explicit code comment; documented in README as local/demo-only. |

---

## Clarifications (questions for a product owner)

Realistic open questions this project would raise before moving beyond Core scope:

1. **Should multiple users per company be supported?**  
   The schema allows it (`ApplicationUser.CompanyId`), but only one user per company is seeded and there is no invite, role-assignment, or user-management UI. Should proprietors be able to add staff with different permissions?

2. **Should quotations be editable or cancellable after creation?**  
   Core scope is create-and-view only. In a real business, users may need to revise line items, extend validity dates, or void a quotation — what is the expected workflow?

3. **Should there be an approval workflow before a quotation is finalized or sent to a customer?**  
   Currently any authenticated user can create and download a PDF immediately. Should drafts, approvals, or email delivery be part of the process?

---

## Edge Cases

Edge cases **handled or considered** in the current implementation:

| Edge case | Handling |
|-----------|----------|
| **Company with no settings yet** | `CompanySettingsService.GetAsync` returns documented defaults: `PrimaryColor` = `#1F2937`, generic `InvoiceTerms` sentence. Covered by `CompanySettingsDefaultsTests`. |
| **Quotation with zero line items** | Rejected — controller strips empty lines and requires ≥ 1 line with `ProductId > 0`; `QuotationService` also returns failure if `Lines` is null or empty. |
| **Customer belonging to a different company** | Rejected — `QuotationService` checks `CustomerId` exists with matching `CompanyId`. |
| **Product belonging to a different company** | Rejected — `QuotationService` validates all `ProductId` values belong to the user's company. |
| **Line item with zero or negative quantity** | Rejected — `QuotationService` returns failure if any line `Quantity <= 0`. |
| **Quotation detail for another company's ID** | Returns `NotFound()` — `GetDetailAsync` filters by both `id` and `companyId`. |
| **Search keyword under 2 characters** | `SearchService` returns empty results with `KeywordTooShort = true`; view shows a friendly message. |
| **Duplicate signing key generation on startup** | Guarded — `SigningKeySeeder` calls `GetActiveKeyAsync()`, which returns an existing active non-expired key or creates one only if none exists. `IdentitySeeder` skips users that already exist. |
| **Unauthenticated access to protected pages** | Redirect to `/Account/Login` via `[Authorize]` and cookie auth configuration. |
| **Authenticated user with no resolvable company** | Controllers return `Challenge()` when `ICurrentCompanyService` returns null. |
| **PDF font rendering ("ti" ligature corruption)** | Fixed in `QuotationPdfService` by setting explicit Arial font family and disabling standard ligatures (`debugging-notes.md` Issue 3). |
| **DbContext concurrency in parallel queries** | Fixed by sequential awaits in `SearchService` and `DashboardService` (`debugging-notes.md` Issues 4 and 6). |

---

## Related Documentation

- [acceptance-criteria.md](acceptance-criteria.md) — Core checklist with evidence
- [design-notes.md](design-notes.md) — Architecture and patterns
- [data-model.md](data-model.md) — Entity model and relationships
- [ui-flow.md](ui-flow.md) — Page navigation and user flows
- [api-contract.md](api-contract.md) — MVC controller actions
