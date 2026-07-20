using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmeErp.Infrastructure.Identity;

namespace SmeErp.Infrastructure.Identity;

public static class IdentitySeeder
{
    public const string AdminRole = "Admin";
    public const string ProprietorRole = "Proprietor";

    // Local/demo use only — replace with secure credentials management in production.
    private const string DemoPassword = "Passw0rd!123";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        await SeedRolesAsync(roleManager, logger);
        await SeedUsersAsync(userManager, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        foreach (var roleName in new[] { AdminRole, ProprietorRole })
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                logger.LogError(
                    "Failed to create role {RoleName}: {Errors}",
                    roleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var users = new[]
        {
            new
            {
                Email = "admin@sharmatrading.com",
                FullName = "Rakesh Sharma",
                CompanyId = 1,
                Role = ProprietorRole
            },
            new
            {
                Email = "admin@vermadist.com",
                FullName = "Priya Verma",
                CompanyId = 2,
                Role = ProprietorRole
            }
        };

        foreach (var seedUser in users)
        {
            var existingUser = await userManager.FindByEmailAsync(seedUser.Email);
            if (existingUser is not null)
            {
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = seedUser.Email,
                Email = seedUser.Email,
                EmailConfirmed = true,
                FullName = seedUser.FullName,
                CompanyId = seedUser.CompanyId
            };

            var createResult = await userManager.CreateAsync(user, DemoPassword);
            if (!createResult.Succeeded)
            {
                logger.LogError(
                    "Failed to create user {Email}: {Errors}",
                    seedUser.Email,
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                continue;
            }

            var roleResult = await userManager.AddToRoleAsync(user, seedUser.Role);
            if (!roleResult.Succeeded)
            {
                logger.LogError(
                    "Failed to assign role {Role} to user {Email}: {Errors}",
                    seedUser.Role,
                    seedUser.Email,
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
