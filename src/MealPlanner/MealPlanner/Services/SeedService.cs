using Microsoft.AspNetCore.Identity;
using MealPlanner.Models;
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
               //Ensure the data is ready
               logger.LogInformation("Ensuring the database is created.");
               await context.Database.EnsureCreatedAsync();

               //Add roles
               logger.LogInformation("Seeding roles.");
               await AddRoleAsync(roleManager, "Admin");
               await AddRoleAsync(roleManager, "User");

               //Add admin user
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
                        logger.LogError("Failed to create admin user");
                    }
                }
                else
                {
                    logger.LogInformation("Failed to create admin user");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding data.");
                throw;
            }
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
       