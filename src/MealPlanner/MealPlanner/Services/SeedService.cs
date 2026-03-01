using Microsoft.AspNetCore.Identity;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Services;

public class SeedService
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<MealPlannerDBContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

        try
        {
            logger.LogInformation("Applying migrations (Database.Migrate).");
            await context.Database.MigrateAsync();

            // ✅ Seed Dietary Restrictions (only if none exist)
            logger.LogInformation("Seeding dietary restrictions.");
            await SeedDietaryRestrictionsAsync(context, logger);

            logger.LogInformation("Seeding roles.");
            await AddRoleAsync(roleManager, "Admin");
            await AddRoleAsync(roleManager, "User");

            logger.LogInformation("Seeding admin user.");
            var adminEmail = "admin@codehub.com";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new User
                {
                    FullName = "Code Hub",
                    UserName = adminEmail,
                    NormalizedUserName = adminEmail.ToUpper(),
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpper(),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    logger.LogInformation("Assigning Admin role to the admin user.");
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding data.");
            throw;
        }
    }

    private static async Task SeedDietaryRestrictionsAsync(MealPlannerDBContext context, ILogger logger)
    {
        if (await context.DietaryRestrictions.AnyAsync())
        {
            logger.LogInformation("Dietary restrictions already exist; skipping seed.");
            return;
        }

        context.DietaryRestrictions.AddRange(
            new DietaryRestriction { Name = "Gluten-Free" },
            new DietaryRestriction { Name = "Dairy-Free" },
            new DietaryRestriction { Name = "Vegetarian" },
            new DietaryRestriction { Name = "Vegan" },
            new DietaryRestriction { Name = "Keto" },
            new DietaryRestriction { Name = "Halal" },
            new DietaryRestriction { Name = "Kosher" },
            new DietaryRestriction { Name = "Nut Allergy" }
        );

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded dietary restrictions.");
    }

    private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create {roleName} role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}