using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Persistence.Seed;

public static class CompanySeedData
{
    public static readonly Company[] Companies =
    {
        new()
        {
            Id = 1,
            Name = "Sharma Trading Co.",
            Tagline = "Your Trusted Partner for Hardware & Electricals",
            Address = "12, MI Road, Near Gandhi Nagar",
            City = "Jaipur",
            State = "Rajasthan",
            Country = "India",
            PinCode = "302001",
            GstNumber = "08AABCS1234A1Z5",
            PanNumber = "AABCS1234A",
            Mobile = "9876543210",
            Email = "contact@sharmatrading.co.in",
            Website = "www.sharmatrading.co.in"
        },
        new()
        {
            Id = 2,
            Name = "Verma Distributors",
            Tagline = "Quality Stationery & Office Supplies",
            Address = "45, FC Road, Shivajinagar",
            City = "Pune",
            State = "Maharashtra",
            Country = "India",
            PinCode = "411005",
            GstNumber = "27AABCV5678B1Z3",
            PanNumber = "AABCV5678B",
            Mobile = "9822012345",
            Email = "sales@vermadistributors.in",
            Website = "www.vermadistributors.in"
        }
    };
}
