# SmeErp Data Model

## Overview

SmeErp uses a shared-database, multi-tenant model. Each **company** is a tenant. Tenant-owned business data carries a `CompanyId` foreign key pointing at `Company`, and application services filter queries by the logged-in user's company. `Company` itself has no `CompanyId` because it **is** the tenant root — the entity that defines each isolated business. `SigningKey` is global (not company-scoped). `QuotationLine` has no `CompanyId`; it is scoped indirectly through its parent `Quotation`.

At runtime, `ICurrentCompanyService` resolves the authenticated user's `CompanyId` (from `ApplicationUser`) so list, search, and create operations only touch that tenant's rows.

## Tenant Scope

| Entity | Scoped by `CompanyId`? | Notes |
|--------|------------------------|-------|
| `Company` | No | Tenant root |
| `CompanySetting` | Yes | FK to `Company` |
| `Product` | Yes | FK to `Company` |
| `Customer` | Yes | FK to `Company` |
| `Quotation` | Yes | FK to `Company` |
| `QuotationLine` | Indirectly | FK to `Quotation` (which has `CompanyId`) |
| `ApplicationUser` | Yes | FK to `Company` |
| `SigningKey` | No | Application-wide signing key storage |

## Indexing Strategy

Every entity that has a `CompanyId` column is indexed on `CompanyId` in Fluent API configuration:

- `CompanySetting` — `HasIndex(s => s.CompanyId)`
- `Product` — `HasIndex(p => p.CompanyId)`
- `Customer` — `HasIndex(c => c.CompanyId)`
- `Quotation` — `HasIndex(q => q.CompanyId)`
- `ApplicationUser` — `HasIndex(u => u.CompanyId)`

This supports efficient tenant-filtered queries (`WHERE CompanyId = @currentCompanyId`), which run on almost every business operation.

`SigningKey` is indexed on `IsActive` instead (`HasIndex(k => k.IsActive)`), since it is not tenant-scoped. `QuotationLine` has no `CompanyId` and therefore no `CompanyId` index.

## Entities

### Company

**Location:** `SmeErp.Domain/Entities/Company.cs`  
**Scope:** Tenant root (no `CompanyId`)

Represents a business tenant. All other tenant-scoped entities reference this table.

| Field | Type | Purpose |
|-------|------|---------|
| `Id` | `int` | Primary key |
| `Name` | `string` | Company name (max 200) |
| `Tagline` | `string` | Marketing tagline (max 500) |
| `Address` | `string` | Street address (max 500) |
| `City` | `string` | City (max 100) |
| `State` | `string` | State (max 100) |
| `Country` | `string` | Country (max 100) |
| `PinCode` | `string` | Postal code (max 20) |
| `GstNumber` | `string` | GST registration number (max 20) |
| `PanNumber` | `string` | PAN number (max 20) |
| `Mobile` | `string` | Contact mobile (max 20) |
| `Email` | `string` | Contact email (max 256) |
| `Website` | `string` | Website URL (max 500) |
| `LogoPath` | `string?` | Optional path to logo file (max 500) |

**Navigation properties:** `Settings`, `Products`, `Customers`, `Quotations`

---

### CompanySetting

**Location:** `SmeErp.Domain/Entities/TenantScoped/CompanySetting.cs`  
**Scope:** Tenant-scoped (`CompanyId`)

Key-value settings for a company (e.g. `PrimaryColor`, `InvoiceTerms`).

| Field | Type | Purpose |
|-------|------|---------|
| `Id` | `int` | Primary key |
| `CompanyId` | `int` | FK to owning `Company` |
| `Key` | `string` | Setting name (max 200) |
| `Value` | `string` | Setting value (max 2000) |

**Navigation properties:** `Company`

**Delete behavior:** `Restrict` on `Company` (deleting a company does not cascade-delete settings).

---

### Product

**Location:** `SmeErp.Domain/Entities/TenantScoped/Product.cs`  
**Scope:** Tenant-scoped (`CompanyId`)

Inventory/catalog item belonging to one company.

| Field | Type | Purpose |
|-------|------|---------|
| `Id` | `int` | Primary key |
| `CompanyId` | `int` | FK to owning `Company` |
| `Name` | `string` | Product name (max 200) |
| `Sku` | `string` | Stock-keeping unit (max 100) |
| `Barcode` | `string` | Barcode (max 100) |
| `SellingPrice` | `decimal` | Unit selling price (precision 18,2) |
| `GstPercent` | `decimal` | GST rate applied to this product (precision 5,2) |
| `CurrentStock` | `int` | Available stock quantity |

**Navigation properties:** `Company`, `QuotationLines`

**Delete behavior:** `Restrict` on `Company`.

---

### Customer

**Location:** `SmeErp.Domain/Entities/TenantScoped/Customer.cs`  
**Scope:** Tenant-scoped (`CompanyId`)

Customer record belonging to one company.

| Field | Type | Purpose |
|-------|------|---------|
| `Id` | `int` | Primary key |
| `CompanyId` | `int` | FK to owning `Company` |
| `Name` | `string` | Customer name (max 200) |
| `Code` | `string` | Customer code (max 50) |
| `Mobile` | `string` | Contact mobile (max 20) |
| `City` | `string` | City (max 100) |
| `State` | `string` | State (max 100) |
| `Address` | `string` | Street address (max 500) |

**Navigation properties:** `Company`, `Quotations`

**Delete behavior:** `Restrict` on `Company`.

---

### Quotation

**Location:** `SmeErp.Domain/Entities/TenantScoped/Quotation.cs`  
**Scope:** Tenant-scoped (`CompanyId`)

A sales quotation header with calculated totals and a link to one customer.

| Field | Type | Purpose |
|-------|------|---------|
| `Id` | `int` | Primary key |
| `CompanyId` | `int` | FK to owning `Company` |
| `QuotationNumber` | `string` | Human-readable quotation number (max 50) |
| `CustomerId` | `int` | FK to `Customer` |
| `QuotationDate` | `DateTime` | Date the quotation was created |
| `ValidUntil` | `DateTime` | Expiry date for the quotation |
| `SubTotal` | `decimal` | Sum of line subtotals before tax (precision 18,2) |
| `TaxAmount` | `decimal` | Total tax across all lines (precision 18,2) |
| `DiscountAmount` | `decimal` | Total discount across all lines (precision 18,2) |
| `TotalAmount` | `decimal` | Grand total (precision 18,2) |
| `Notes` | `string?` | Optional free-text notes (max 2000) |

**Navigation properties:** `Company`, `Customer`, `Lines`

**Delete behavior:** `Restrict` on both `Company` and `Customer`.

---

### QuotationLine

**Location:** `SmeErp.Domain/Entities/TenantScoped/QuotationLine.cs`  
**Scope:** Indirectly tenant-scoped (via `QuotationId`; no `CompanyId` column)

A single line item on a quotation, referencing a product with quantity, pricing, and calculated amounts.

| Field | Type | Purpose |
|-------|------|---------|
| `Id` | `int` | Primary key |
| `QuotationId` | `int` | FK to parent `Quotation` |
| `ProductId` | `int` | FK to `Product` |
| `Quantity` | `int` | Units quoted |
| `UnitPrice` | `decimal` | Price per unit at time of quotation (precision 18,2) |
| `DiscountPercent` | `decimal` | Line discount percentage (precision 5,2) |
| `GstPercent` | `decimal` | GST rate applied on this line (precision 5,2) |
| `TaxAmount` | `decimal` | Calculated tax for this line (precision 18,2) |
| `TotalAmount` | `decimal` | Calculated line total (precision 18,2) |

**Navigation properties:** `Quotation`, `Product`

**Delete behavior:** `Cascade` when parent `Quotation` is deleted; `Restrict` on `Product`.

---

### ApplicationUser

**Location:** `SmeErp.Infrastructure/Identity/ApplicationUser.cs` (extends `IdentityUser`)  
**Scope:** Tenant-scoped (`CompanyId`)

ASP.NET Identity user linked to exactly one company. Inherits standard Identity fields from `IdentityUser` (e.g. `Id`, `UserName`, `Email`, `PasswordHash`).

| Field | Type | Purpose |
|-------|------|---------|
| `FullName` | `string` | Display name (max 200) |
| `CompanyId` | `int` | FK to `Company` — determines which tenant's data this user can access |

**Navigation properties:** None defined on the entity; Fluent API configures a required FK to `Company` without a collection on `Company`.

**Delete behavior:** `Restrict` on `Company`.

---

### SigningKey

**Location:** `SmeErp.Domain/Entities/SigningKey.cs`  
**Scope:** Global (no `CompanyId`)

Stores cryptographically generated signing keys for the application (not per-tenant). Keys are created and rotated at runtime by `SigningKeyService`.

| Field | Type | Purpose |
|-------|------|---------|
| `Id` | `int` | Primary key |
| `KeyValue` | `string` | Base64-encoded secret (max 500) |
| `CreatedAt` | `DateTime` | When the key was generated |
| `ExpiresAt` | `DateTime` | When the key expires |
| `IsActive` | `bool` | Whether this key is the current active key |

**Navigation properties:** None.

**Indexing:** `HasIndex(k => k.IsActive)` to quickly find the active key.

---

## Relationships

```
Company (tenant root)
 ├── 1:N  CompanySetting
 ├── 1:N  Product
 ├── 1:N  Customer
 ├── 1:N  Quotation
 └── 1:N  ApplicationUser

Customer
 └── 1:N  Quotation

Quotation
 └── 1:N  QuotationLine

Product
 └── 1:N  QuotationLine

SigningKey
 └── (no relationships)
```

### Relationship detail

| From | To | Cardinality | FK column | Delete behavior |
|------|----|-------------|-----------|-----------------|
| `CompanySetting` | `Company` | N:1 | `CompanyId` | Restrict |
| `Product` | `Company` | N:1 | `CompanyId` | Restrict |
| `Customer` | `Company` | N:1 | `CompanyId` | Restrict |
| `Quotation` | `Company` | N:1 | `CompanyId` | Restrict |
| `Quotation` | `Customer` | N:1 | `CustomerId` | Restrict |
| `QuotationLine` | `Quotation` | N:1 | `QuotationId` | Cascade |
| `QuotationLine` | `Product` | N:1 | `ProductId` | Restrict |
| `ApplicationUser` | `Company` | N:1 | `CompanyId` | Restrict |

`Restrict` on `Company` prevents accidental cascade deletion of a tenant's data. `Cascade` on `Quotation` → `QuotationLine` ensures line items are removed when their parent quotation is deleted.
