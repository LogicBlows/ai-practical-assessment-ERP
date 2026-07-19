using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Persistence.Seed;

public static class CompanySettingSeedData
{
    public static readonly CompanySetting[] Settings =
    {
        new()
        {
            Id = 1,
            CompanyId = 1,
            Key = "PrimaryColor",
            Value = "#1E40AF"
        },
        new()
        {
            Id = 2,
            CompanyId = 1,
            Key = "InvoiceTerms",
            Value = "Payment due within 15 days of invoice date. Goods once sold will not be taken back. Interest @ 18% p.a. will be charged on overdue amounts."
        },
        new()
        {
            Id = 3,
            CompanyId = 2,
            Key = "PrimaryColor",
            Value = "#047857"
        },
        new()
        {
            Id = 4,
            CompanyId = 2,
            Key = "InvoiceTerms",
            Value = "Payment due within 30 days of invoice date. Shortages must be reported within 48 hours of delivery. All disputes subject to Pune jurisdiction."
        }
    };
}
