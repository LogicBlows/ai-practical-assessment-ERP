# SmeErp UI Flow

## Overview

SmeErp is a server-rendered Razor MVC application. Users interact through full page loads and form submissions — there is no SPA or AJAX API layer. Navigation is driven by the top navbar in `_Layout.cshtml`, which changes based on whether the user is signed in.

---

## 1. Navigation Structure

### Navbar brand (always visible)

- **Link:** company name (from settings when signed in) or `"SmeErp"` → `/` (Home/Index)

### Authenticated users only

| Nav item | Route | Purpose |
|----------|-------|---------|
| Dashboard | `/Dashboard` | KPI overview |
| Products | `/Products` | Product list and search |
| Customers | `/Customers` | Customer list and search |
| Quotations | `/Quotations` | Quotation list |
| Settings | `/Settings` | Company profile and branding |
| Search box + **Go** | `GET /Search?keyword=...` | Global search |
| **Sign out** | `POST /Account/Logout` | Ends session → Login page |

The navbar background color is driven by the signed-in company's `PrimaryColor` setting (via `CompanyBrandingViewComponent`).

### Unauthenticated users only

| Nav item | Route |
|----------|-------|
| Home | `/` |
| Sign in | `/Account/Login` |

### Footer (always visible)

- Privacy link → `/Home/Privacy`

### Pages not in the main nav

These are reached by links, redirects, or direct URLs:

- `/Account/Login` — sign-in form
- `/Quotations/Create` — new quotation form (from Quotations list)
- `/Quotations/Details/{id}` — quotation detail (from list, search, or post-create redirect)
- `/Quotations/DownloadPdf/{id}` — PDF file download (from Details page)
- `/Search` — full search results page (from nav search box or direct visit)

---

## 2. Login Flow

```
Unauthenticated user visits a protected page (e.g. /Dashboard)
  → ASP.NET Identity redirects to /Account/Login

User enters email + password on Login page
  → POST /Account/Login

On success
  → Redirect to /Dashboard/Index
     (or to returnUrl if a valid local URL was provided)

On failure
  → Stay on Login page with "Invalid email or password."

Already signed in and visits /Account/Login
  → Redirect to /Dashboard/Index
```

**Sign out:**

```
Authenticated user clicks "Sign out" in navbar
  → POST /Account/Logout
  → Redirect to /Account/Login
```

Demo accounts (each sees only their own company's data):

| Company | Email |
|---------|-------|
| Sharma Trading Co. | `admin@sharmatrading.com` |
| Verma Distributors | `admin@vermadist.com` |

---

## 3. Page Flows

### Home (`/`) — unauthenticated landing

1. User visits `/` without signing in.
2. Sees a welcome page with a link to ASP.NET Core documentation.
3. User can click **Sign in** in the navbar to reach Login.

No business data is shown. Authenticated users typically use Dashboard instead (login redirects there).

---

### Dashboard (`/Dashboard`)

**How to get there:** Login success redirect, or **Dashboard** nav link.

**What the user sees:** Four KPI cards for their company:

- Total Products
- Total Customers
- Quotations Today
- Pending Quotations

**Actions:** Read-only overview. User navigates elsewhere via the navbar. No links on the cards themselves.

---

### Products (`/Products`)

**How to get there:** **Products** nav link, or a product result from Global Search (`/Products?search=...`).

**What the user can do:**

1. View a table of all products for their company (name, SKU, barcode, price, GST %, stock).
2. Search by name, SKU, or barcode using the search box → `GET /Products?search={term}`.
3. Clear search → back to `/Products` (full list).

**Where actions lead:** List-only page — no create/edit/detail links. Rows are not clickable.

---

### Customers (`/Customers`)

**How to get there:** **Customers** nav link, or a customer result from Global Search (`/Customers?search=...`).

**What the user can do:**

1. View a table of all customers for their company (name, code, mobile, city, state, address).
2. Search by name or code → `GET /Customers?search={term}`.
3. Clear search → back to `/Customers`.

**Where actions lead:** List-only page — no create/edit/detail links.

---

### Quotations Index (`/Quotations`)

**How to get there:** **Quotations** nav link.

**What the user can do:**

1. View a table of quotations for their company (number, customer, date, total), newest first.
2. Click **View** on a row → `/Quotations/Details/{id}`.
3. Click **New quotation** (top right) → `/Quotations/Create`.

**Where actions lead:**

```
/Quotations
  → "New quotation" → /Quotations/Create
  → "View"          → /Quotations/Details/{id}
```

---

### Quotations Create (`/Quotations/Create`)

**How to get there:** **New quotation** button on Quotations Index.

**What the user can do:**

1. Select a customer from a dropdown (company-scoped).
2. Set quotation date and valid-until date.
3. Optionally enter notes.
4. Add one or more line items (product, quantity, unit price, discount %). Client-side JS can add/remove rows and auto-fill unit price from the selected product.
5. Submit → `POST /Quotations/Create`.
6. Cancel → back to `/Quotations`.

**Where actions lead:**

```
Submit (valid)
  → Redirect to /Quotations/Details/{newId}

Submit (invalid)
  → Re-render Create form with validation errors

Cancel
  → /Quotations
```

---

### Quotations Details (`/Quotations/Details/{id}`)

**How to get there:**

- **View** on Quotations Index
- Redirect after successful Create
- Quotation result from Global Search (`/Quotations/Details/{id}`)

**What the user can do:**

1. View read-only quotation header (customer, dates, notes).
2. View line items table (product, qty, unit price, discount, GST, tax, line total).
3. View totals (subtotal, discount, tax, grand total).
4. Click **Download PDF** → `GET /Quotations/DownloadPdf/{id}` → browser downloads `{QuotationNumber}.pdf`.
5. Click **Back to list** → `/Quotations`.

**Where actions lead:**

```
/Quotations/Details/{id}
  → "Download PDF"  → PDF file download (application/pdf)
  → "Back to list"    → /Quotations
```

If the quotation does not belong to the user's company → `404 Not Found`.

---

### Settings (`/Settings`)

**How to get there:** **Settings** nav link.

**What the user can do:**

1. View and edit company profile fields (name, address, city, state, country, PIN, GST, PAN, mobile, email, website).
2. Pick a **Primary color** (HTML color picker) — updates navbar accent on next page load.
3. Edit **Invoice terms** text — used in quotation PDF footers.
4. Submit → `POST /Settings` → redirect back to `/Settings` with a success message.

**Where actions lead:**

```
Save (valid)   → /Settings (with "Company settings saved successfully.")
Save (invalid) → Re-render form with validation errors
```

Changes to address and `PrimaryColor` are reflected on the **next** quotation PDF download (settings are loaded fresh each time).

---

### Search (`/Search`)

**How to get there:**

- Submit keyword in navbar search box → `GET /Search?keyword={term}`
- Visit `/Search` directly (shows prompt to enter a keyword)

Covered in detail in section 4 below.

---

## 4. Global Search Flow

```
User types keyword in navbar search box → clicks "Go"
  → GET /Search?keyword={term}
  → Search results page loads

If keyword is empty
  → Prompt: "Enter a keyword above to search..."

If keyword is 1 character
  → Message: "Enter at least 2 characters to search."

If keyword is 2+ characters and no matches
  → Message: "No results found for '{keyword}'."

If keyword is 2+ characters and matches exist
  → Results grouped under headings:
       Products   → links to /Products?search={productName}
       Customers  → links to /Customers?search={customerName}
       Quotations → links to /Quotations/Details/{id}
```

The search page also has its own search form to refine or rerun the query.

**Example:**

```
Navbar: keyword "cable" → Go
  → /Search?keyword=cable
  → Products section: "Copper Cable 2.5mm" → /Products?search=Copper%20Cable%202.5mm
  → Customers section: (if any match)
  → Quotations section: "QT-1-00002" → /Quotations/Details/5
```

All search results are scoped to the signed-in user's company only.

---

## 5. Multi-Tenant Scoping

Every authenticated business page filters data by the logged-in user's `CompanyId` (resolved via `ICurrentCompanyService` from `ApplicationUser.CompanyId`). Two users from different companies never see each other's data.

| Page / feature | What differs per company |
|----------------|--------------------------|
| **Navbar brand title** | Company name from settings |
| **Navbar color** | `PrimaryColor` from settings |
| **Dashboard KPIs** | Counts of that company's products, customers, and quotations |
| **Products list** | Only that company's products (4 seeded per company) |
| **Customers list** | Only that company's customers (3 seeded per company) |
| **Quotations list** | Only that company's quotations |
| **Create quotation** | Customer and product dropdowns show only that company's records |
| **Quotation details / PDF** | Returns `404` if quotation ID belongs to another company |
| **Settings** | Loads and saves only the current user's company profile |
| **Global search** | Matches only products, customers, and quotations for that company |

**Concrete example with seeded data:**

```
admin@sharmatrading.com (CompanyId = 1, Sharma Trading Co.)
  → Products: hardware/electricals (Jaipur customers)
  → Dashboard: reflects Sharma's quotation counts

admin@vermadist.com (CompanyId = 2, Verma Distributors)
  → Products: stationery/office supplies (Pune customers)
  → Dashboard: different counts; zero of Sharma's quotations visible
```

A Verma user searching or browsing will never see Sharma products, customers, or quotations, and vice versa. Cross-tenant quotation access by ID returns `NotFound`, not another company's data.

---

## End-to-End Example: Create and Download a Quotation

```
1. Sign in → /Account/Login → Dashboard
2. Quotations nav → /Quotations
3. "New quotation" → /Quotations/Create
4. Select customer, add line items, submit
5. Redirect → /Quotations/Details/{id}
6. "Download PDF" → PDF file downloads
7. Optional: Settings → change PrimaryColor → repeat step 6 → PDF reflects new color
```

---

## Related Documentation

- [api-contract.md](api-contract.md) — controller routes, inputs, and responses
- [design-notes.md](design-notes.md) — architecture and tenant isolation design
- [README.md](README.md) — setup and demo login credentials
