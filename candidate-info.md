# Candidate Information

Name: Ashish Spencer
Role: Senior Software Engineer
Primary Technology Stack: .NET / C# / SQL Server
Primary AI Tool Used: Cursor
Project Option Selected: Option 3 — .NET Full-Stack SME ERP (Inventory Management)

Assessment Start Date: 2026-07-19
Submission Date: 2026-07-21

## Project Summary
Built a small multi-tenant ERP application (SmeErp) for trading/
distribution businesses, covering authentication, role-based access,
product/customer management, quotation creation with automatic tax
calculation, branded PDF generation driven by company settings, global
search, and dashboard KPIs — all scoped per-company via a CompanyId-based
multi-tenant architecture. Extended beyond the base Core requirements
to include a DB-stored, auto-rotating JWT signing key (per a separate
stakeholder requirement) and a two-company seeded demo (Sharma Trading
Co. and Verma Distributors) to concretely demonstrate tenant isolation.

## Tools Used
Cursor (primary AI pair-programming tool), SQL Server Management
Studio (database verification), Git/GitHub (version control, feature
branches, pull requests).

## Setup Summary
See README.md for full setup instructions. In brief: clone repo,
configure SQL Server connection string, run EF Core migrations,
`dotnet run` from src/SmeErp.Web, log in with seeded credentials.