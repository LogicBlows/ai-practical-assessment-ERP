using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmeErp.Infrastructure.Persistence.Migrations
{
    public partial class SeedProductsCustomersSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CompanySettings",
                columns: new[] { "Id", "CompanyId", "Key", "Value" },
                values: new object[,]
                {
                    { 1, 1, "PrimaryColor", "#1E40AF" },
                    { 2, 1, "InvoiceTerms", "Payment due within 15 days of invoice date. Goods once sold will not be taken back. Interest @ 18% p.a. will be charged on overdue amounts." },
                    { 3, 2, "PrimaryColor", "#047857" },
                    { 4, 2, "InvoiceTerms", "Payment due within 30 days of invoice date. Shortages must be reported within 48 hours of delivery. All disputes subject to Pune jurisdiction." }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "City", "Code", "CompanyId", "Mobile", "Name", "State" },
                values: new object[,]
                {
                    { 1, "Shop 14, Chandpole Bazaar", "Jaipur", "CUST-S1-001", 1, "9414012345", "Rajesh Electricals", "Rajasthan" },
                    { 2, "B-22, Vaishali Nagar, Near Ajmer Road", "Jaipur", "CUST-S1-002", 1, "9829015678", "Pink City Builders", "Rajasthan" },
                    { 3, "18, Malviya Nagar Industrial Area", "Jaipur", "CUST-S1-003", 1, "9988776655", "Malviya Nagar Hardware Store", "Rajasthan" },
                    { 4, "Shop 7, Karve Road, Kothrud", "Pune", "CUST-V2-001", 2, "9890011223", "Kothrud Office Mart", "Maharashtra" },
                    { 5, "Plot 12, Sinhagad Road, Dhayari", "Pune", "CUST-V2-002", 2, "9765432109", "Sinhagad Road Enterprises", "Maharashtra" },
                    { 6, "45, JM Road, Deccan Gymkhana", "Pune", "CUST-V2-003", 2, "9823045678", "Deccan Stationers", "Maharashtra" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Barcode", "CompanyId", "CurrentStock", "GstPercent", "Name", "SellingPrice", "Sku" },
                values: new object[,]
                {
                    { 1, "8901030575123", 1, 240, 18.00m, "Havells 6A Modular Switch", 85.00m, "HW-SW-6A-001" },
                    { 2, "8901396123456", 1, 36, 18.00m, "Polycab 2.5 sq mm FR Cable (90m)", 2450.00m, "PC-CBL-2.5-90" },
                    { 3, "8901396789012", 1, 180, 18.00m, "Anchor Roma 5A Socket", 62.00m, "AN-SKT-5A-001" },
                    { 4, "8718696123456", 1, 320, 18.00m, "Philips LED Bulb 9W Cool Daylight", 95.00m, "PH-LED-9W-CDL" },
                    { 5, "8901030456789", 2, 500, 12.00m, "Classmate Notebook A4 (200 Pages)", 65.00m, "CM-NB-A4-200" },
                    { 6, "8901396234567", 2, 275, 12.00m, "Reynolds Trimax Pen (Pack of 10)", 120.00m, "RL-TM-PK10" },
                    { 7, "8901396345678", 2, 90, 18.00m, "Kangaro Stapler DP-500", 185.00m, "KG-ST-DP500" },
                    { 8, "8901396456789", 2, 150, 12.00m, "JK Copier Paper A4 (500 Sheets)", 310.00m, "JK-CP-A4-500" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
