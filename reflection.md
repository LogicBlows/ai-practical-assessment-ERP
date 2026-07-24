# Reflection

## What I Built
A multi-tenant ERP application (SmeErp) for small trading/distribution
businesses — authentication and role-based access, product and
customer management, quotation creation with automatic tax/discount
calculation, branded PDF generation driven by company settings, global
search, and dashboard KPIs. Two fictional companies (Sharma Trading Co.
and Verma Distributors) were seeded specifically to make multi-tenant
data isolation demonstrable rather than just architecturally claimed.
Beyond Core scope, a DB-stored, auto-generated JWT signing key with
rotation support was added, based on a separate real-world requirement
from a supervisor.

## How I Used AI (across the lifecycle)
Cursor was used across the entire lifecycle: scaffolding the solution
with architectural constraints established upfront, generating entities
and DbContext configuration, implementing each business feature on its
own branch, generating documentation grounded in the actual codebase,
and even logging its own work into planning.md and debugging-notes.md
once that pattern was established partway through the project.

## What AI Helped With Most
Cursor was strongest at producing structurally consistent code once
architectural rules were established early — the Clean Architecture
layering, ServiceResult<T> pattern, and multi-tenant CompanyId
convention were applied correctly and consistently across nearly every
feature generated afterward, without needing to be re-specified each
time. It was also effective at quickly drafting documentation grounded
in the real codebase once code existed to read from.

## What AI Got Wrong
Several concrete failures occurred and were caught through direct
verification rather than assumption:
- A multi-part seed data prompt was only partially completed (only
  Company data was seeded; Product, Customer, and CompanySetting were
  silently skipped) — caught by inspecting the database directly in
  SQL Server Management Studio, not by trusting the stated summary.
- The same EF Core anti-pattern (Task.WhenAll executed against a
  single, shared, non-thread-safe DbContext) was introduced
  independently in two separate features (Global Search and Dashboard
  KPIs), both producing runtime exceptions caught only through manual
  testing in the browser.
- A required DI service registration was omitted, silently breaking an
  entire controller (all Quotation pages) until manually clicked
  through and diagnosed from the actual exception stack trace.
- A default font library setting caused specific character sequences
  to be silently dropped from generated PDF text, visible only by
  actually reading the output PDF rather than confirming the build
  succeeded.
- Documentation-generation prompts twice wrote content to an incorrect
  file location (ai-prompts/debugging-notes.md instead of the required
  root-level debugging-notes.md), requiring manual correction and
  de-duplication.

## How I Validated AI Output
Validation was never based on "the build succeeded" or "the agent said
it worked." Every feature was manually exercised in the running
application — logging in as both seeded tenant accounts to confirm data
isolation, hand-calculating expected quotation totals against actual
app output, directly querying the database after every seed/migration
step, and reading actual generated files (PDFs, markdown logs) rather
than trusting a natural-language summary of what was supposedly done.

## What I Would Improve Next
I would introduce automated integration tests earlier in the process —
several of the bugs found manually (the DbContext concurrency issue in
particular, which recurred independently in two features) would likely
have been caught immediately by a basic integration test suite running
against each new service, rather than being discovered one feature at a
time through manual browser testing. I would also establish the
documentation-generation workflow (having Cursor log its own prompts
into the lifecycle files) from the very first prompt, rather than
partway through, to avoid the retroactive cleanup required once file
location and duplication issues surfaced.

## Reusable Workflow (prompts, rules, specs, templates)
The most reusable pattern from this project is establishing
architectural constraints once, explicitly, in the first prompt of a
new codebase — and then scoping every subsequent prompt to a single,
narrow, independently reviewable concern (one feature, one bug fix, one
documentation file at a time) rather than requesting broad, multi-part
changes. Equally reusable is the discipline of validating against real
system state (database contents, actual application behavior, actual
file output) rather than an AI tool's self-reported success — this
caught every significant issue in this project and would transfer
directly to any AI-assisted development work going forward.