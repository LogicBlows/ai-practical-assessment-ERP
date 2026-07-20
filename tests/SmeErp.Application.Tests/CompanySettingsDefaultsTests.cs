using Microsoft.EntityFrameworkCore;
using SmeErp.Domain.Entities;
using SmeErp.Infrastructure.Persistence;
using SmeErp.Infrastructure.Services;

namespace SmeErp.Application.Tests;

public class CompanySettingsDefaultsTests
{
    private const string ExpectedDefaultPrimaryColor = "#1F2937";
    private const string ExpectedDefaultInvoiceTerms =
        "Payment is due within the agreed credit period. Goods once sold are not returnable unless agreed in writing.";

    [Fact]
    public async Task GetAsync_WhenCompanyHasNoSettings_ReturnsDocumentedDefaults()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);
        dbContext.Companies.Add(new Company
        {
            Id = 100,
            Name = "New Company Ltd",
            Tagline = "Test",
            Address = "1 Test Street",
            City = "Mumbai",
            State = "Maharashtra",
            Country = "India",
            PinCode = "400001",
            GstNumber = "29ABCDE1234F1Z5",
            PanNumber = "ABCDE1234F",
            Mobile = "9000000000",
            Email = "test@example.com",
            Website = "https://example.com"
        });
        await dbContext.SaveChangesAsync();

        var service = new CompanySettingsService(dbContext);

        var result = await service.GetAsync(100);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(ExpectedDefaultPrimaryColor, result.Data.PrimaryColor);
        Assert.Equal(ExpectedDefaultInvoiceTerms, result.Data.InvoiceTerms);
    }
}
