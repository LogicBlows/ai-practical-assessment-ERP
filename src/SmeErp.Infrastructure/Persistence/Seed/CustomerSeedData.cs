using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Persistence.Seed;

public static class CustomerSeedData
{
    public static readonly Customer[] Customers =
    {
        new()
        {
            Id = 1,
            CompanyId = 1,
            Name = "Rajesh Electricals",
            Code = "CUST-S1-001",
            Mobile = "9414012345",
            City = "Jaipur",
            State = "Rajasthan",
            Address = "Shop 14, Chandpole Bazaar"
        },
        new()
        {
            Id = 2,
            CompanyId = 1,
            Name = "Pink City Builders",
            Code = "CUST-S1-002",
            Mobile = "9829015678",
            City = "Jaipur",
            State = "Rajasthan",
            Address = "B-22, Vaishali Nagar, Near Ajmer Road"
        },
        new()
        {
            Id = 3,
            CompanyId = 1,
            Name = "Malviya Nagar Hardware Store",
            Code = "CUST-S1-003",
            Mobile = "9988776655",
            City = "Jaipur",
            State = "Rajasthan",
            Address = "18, Malviya Nagar Industrial Area"
        },
        new()
        {
            Id = 4,
            CompanyId = 2,
            Name = "Kothrud Office Mart",
            Code = "CUST-V2-001",
            Mobile = "9890011223",
            City = "Pune",
            State = "Maharashtra",
            Address = "Shop 7, Karve Road, Kothrud"
        },
        new()
        {
            Id = 5,
            CompanyId = 2,
            Name = "Sinhagad Road Enterprises",
            Code = "CUST-V2-002",
            Mobile = "9765432109",
            City = "Pune",
            State = "Maharashtra",
            Address = "Plot 12, Sinhagad Road, Dhayari"
        },
        new()
        {
            Id = 6,
            CompanyId = 2,
            Name = "Deccan Stationers",
            Code = "CUST-V2-003",
            Mobile = "9823045678",
            City = "Pune",
            State = "Maharashtra",
            Address = "45, JM Road, Deccan Gymkhana"
        }
    };
}
