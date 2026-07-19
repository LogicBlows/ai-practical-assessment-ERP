using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Persistence.Seed;

public static class ProductSeedData
{
    public static readonly Product[] Products =
    {
        new()
        {
            Id = 1,
            CompanyId = 1,
            Name = "Havells 6A Modular Switch",
            Sku = "HW-SW-6A-001",
            Barcode = "8901030575123",
            SellingPrice = 85.00m,
            GstPercent = 18.00m,
            CurrentStock = 240
        },
        new()
        {
            Id = 2,
            CompanyId = 1,
            Name = "Polycab 2.5 sq mm FR Cable (90m)",
            Sku = "PC-CBL-2.5-90",
            Barcode = "8901396123456",
            SellingPrice = 2450.00m,
            GstPercent = 18.00m,
            CurrentStock = 36
        },
        new()
        {
            Id = 3,
            CompanyId = 1,
            Name = "Anchor Roma 5A Socket",
            Sku = "AN-SKT-5A-001",
            Barcode = "8901396789012",
            SellingPrice = 62.00m,
            GstPercent = 18.00m,
            CurrentStock = 180
        },
        new()
        {
            Id = 4,
            CompanyId = 1,
            Name = "Philips LED Bulb 9W Cool Daylight",
            Sku = "PH-LED-9W-CDL",
            Barcode = "8718696123456",
            SellingPrice = 95.00m,
            GstPercent = 18.00m,
            CurrentStock = 320
        },
        new()
        {
            Id = 5,
            CompanyId = 2,
            Name = "Classmate Notebook A4 (200 Pages)",
            Sku = "CM-NB-A4-200",
            Barcode = "8901030456789",
            SellingPrice = 65.00m,
            GstPercent = 12.00m,
            CurrentStock = 500
        },
        new()
        {
            Id = 6,
            CompanyId = 2,
            Name = "Reynolds Trimax Pen (Pack of 10)",
            Sku = "RL-TM-PK10",
            Barcode = "8901396234567",
            SellingPrice = 120.00m,
            GstPercent = 12.00m,
            CurrentStock = 275
        },
        new()
        {
            Id = 7,
            CompanyId = 2,
            Name = "Kangaro Stapler DP-500",
            Sku = "KG-ST-DP500",
            Barcode = "8901396345678",
            SellingPrice = 185.00m,
            GstPercent = 18.00m,
            CurrentStock = 90
        },
        new()
        {
            Id = 8,
            CompanyId = 2,
            Name = "JK Copier Paper A4 (500 Sheets)",
            Sku = "JK-CP-A4-500",
            Barcode = "8901396456789",
            SellingPrice = 310.00m,
            GstPercent = 12.00m,
            CurrentStock = 150
        }
    };
}
