using Microsoft.Extensions.DependencyInjection;
using SmeErp.Infrastructure.Services;

namespace SmeErp.Infrastructure.Persistence.Seed;

public static class SigningKeySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var signingKeyService = scope.ServiceProvider.GetRequiredService<ISigningKeyService>();
        await signingKeyService.GetActiveKeyAsync();
    }
}
