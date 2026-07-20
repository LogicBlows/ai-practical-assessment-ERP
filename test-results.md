# Test Results

## Run Date
2026-07-21

## Command
```bash
dotnet test
```

## Environment
- .NET SDK: 6.0.36
- Test Framework: xUnit.net (VSTest Adapter v2.4.3)
- Projects built: SmeErp.Domain, SmeErp.Shared, SmeErp.Application, SmeErp.Infrastructure, SmeErp.Application.Tests

## Summary
| Total | Failed | Succeeded | Skipped | Duration |
|-------|--------|-----------|---------|----------|
| 2     | 0      | 2         | 0       | 3.6s     |

Build succeeded in 9.7s. All projects restored and compiled without errors or warnings.

## Tests

### 1. Quotation Calculation Test
**Status:** Passed
**Purpose:** Verifies that creating a quotation with known line item inputs (Quantity=3, UnitPrice=62.00, DiscountPercent=6%, GstPercent=18%) produces the correct calculated values: SubTotal 186.00, DiscountAmount 11.16, TaxAmount 31.47, TotalAmount 206.31. These figures were independently hand-calculated and cross-checked against a real quotation generated in the UI (QT-1-00002) during manual testing.

### 2. Settings Defaults Test
**Status:** Passed
**Purpose:** Verifies that CompanySettingsService.GetAsync returns documented default values (PrimaryColor "#1F2937" and a generic InvoiceTerms sentence) when a company has no CompanySetting rows yet, rather than throwing an exception or returning null.

## Notes
Both mandatory xUnit tests required by the assessment pass successfully. No additional Stretch-tier tests are included in this run — see test-strategy.md for what is and isn't covered and why.
