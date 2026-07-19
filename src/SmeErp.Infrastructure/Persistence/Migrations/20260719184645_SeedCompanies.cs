using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmeErp.Infrastructure.Persistence.Migrations
{
    public partial class SeedCompanies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "City", "Country", "Email", "GstNumber", "LogoPath", "Mobile", "Name", "PanNumber", "PinCode", "State", "Tagline", "Website" },
                values: new object[] { 1, "12, MI Road, Near Gandhi Nagar", "Jaipur", "India", "contact@sharmatrading.co.in", "08AABCS1234A1Z5", null, "9876543210", "Sharma Trading Co.", "AABCS1234A", "302001", "Rajasthan", "Your Trusted Partner for Hardware & Electricals", "www.sharmatrading.co.in" });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "City", "Country", "Email", "GstNumber", "LogoPath", "Mobile", "Name", "PanNumber", "PinCode", "State", "Tagline", "Website" },
                values: new object[] { 2, "45, FC Road, Shivajinagar", "Pune", "India", "sales@vermadistributors.in", "27AABCV5678B1Z3", null, "9822012345", "Verma Distributors", "AABCV5678B", "411005", "Maharashtra", "Quality Stationery & Office Supplies", "www.vermadistributors.in" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
