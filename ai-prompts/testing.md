# SmeErp Testing Prompt Log

Copy of the mandatory testing prompt from `ai-prompts/planning.md`.
Original numbering preserved. See `planning.md` for the complete chronological log.

---

## Prompt #14 — Mandatory xUnit Tests (Quotation Calculation, Settings Defaults)
**Date:** 2026-07-21

**Prompt:**
Add the two mandatory xUnit tests to the SmeErp.Application.Tests
project, per the assessment requirements:

1. Quotation Calculation Test:
   Test that creating a quotation with known line items produces the
   correct SubTotal, DiscountAmount, TaxAmount, and TotalAmount.
   Use a concrete example: one line item with Quantity=3, UnitPrice=62.00,
   DiscountPercent=6, GstPercent=18. Assert the calculated values match:
   SubTotal=186.00, DiscountAmount=11.16, TaxAmount=31.47,
   TotalAmount=206.31 (matching the real calculation logic already used
   in QuotationService — extract the calculation into a small, directly
   testable method/class if it's not already isolated from EF Core
   dependencies, so this test doesn't require a live database).

2. Settings Defaults Test:
   Test that when a company has no CompanySetting rows yet (a new/edge
   case company), CompanySettingsService.GetAsync returns the documented
   default values (PrimaryColor "#1F2937" and the generic default
   InvoiceTerms sentence) rather than throwing or returning null. Use an
   in-memory EF Core provider (Microsoft.EntityFrameworkCore.InMemory)
   or a mocked repository to avoid needing a real SQL Server connection
   for this test.

Add the InMemory EF Core NuGet package to the test project if needed.
After writing both tests, run 'dotnet test' and confirm both pass.
Record the test names and pass/fail result.

**Response summary:**
Cursor extracted quotation total calculation into
`QuotationTotalsCalculator` (and supporting input/result types) in
SmeErp.Application/Services/, and refactored QuotationService to call
`QuotationTotalsCalculator.CalculateLine` so the logic is testable without
EF Core. Added `QuotationCalculationTests.Calculate_WithKnownLineItem_ProducesExpectedTotals`
asserting SubTotal=186.00, DiscountAmount=11.16, TaxAmount=31.47, and
TotalAmount=206.31 for the specified line item. Added
`CompanySettingsDefaultsTests.GetAsync_WhenCompanyHasNoSettings_ReturnsDocumentedDefaults`
using an in-memory AppDbContext with a company that has no CompanySetting
rows, asserting PrimaryColor "#1F2937" and the generic default InvoiceTerms
sentence. Added Microsoft.EntityFrameworkCore.InMemory 6.0.36 and an
Infrastructure project reference to SmeErp.Application.Tests. `dotnet test`
confirmed 2/2 tests passing (Total: 2, Failed: 0, Succeeded: 2,
Duration: 3.6s). Test names and results were recorded in test-results.md.

**Accepted / Changed / Rejected:**
- Accepted: both mandatory xUnit tests, calculation extraction to
  Application layer, InMemory EF for settings defaults test.
- Changed: QuotationService now delegates line calculations to
  QuotationTotalsCalculator; test project references Infrastructure.
- Rejected: none.

**Commit:** "Add mandatory xUnit tests for quotation calculation and settings defaults (prompt #14)"
