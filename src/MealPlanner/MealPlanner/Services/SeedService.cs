using Microsoft.AspNetCore.Identity;
using MealPlanner.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;



namespace MealPlanner.Services;

    /// <summary>
    /// SeedService is responsible for seeding the database with initial roles and users.
    /// This includes creating default roles like "Admin" and "User" and a default admin account.
    /// It is typically used during application startup to ensure the database is ready.
    /// </summary>
    public class SeedService
    {

         /// <summary>
        /// Seeds roles and an initial admin user into the database.
        /// This method uses dependency injection to get the required services.
        /// </summary>
        /// <param name="serviceProvider">The IServiceProvider from which to retrieve required services.</param>
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            // Create a new scope to ensure scoped services (like DbContext) are properly disposed after seeding
            using var scope = serviceProvider.CreateScope();

            // Retrieve the application's DbContext from the service provider
            var context = scope.ServiceProvider.GetRequiredService<MealPlannerDBContext>();

            // Retrieve the RoleManager to manage ASP.NET Identity roles
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Retrieve the UserManager to manage ASP.NET Identity users
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Retrieve a logger instance for logging information and errors during seeding
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                // Ensure the database exists and all migrations are applied
               logger.LogInformation("Ensuring the database is created.");
               await context.Database.EnsureCreatedAsync();

               // Seed the required roles in the system
               logger.LogInformation("Seeding roles.");
               await AddRoleAsync(roleManager, "Admin"); // Ensure Admin role exists
               await AddRoleAsync(roleManager, "User"); // Ensure User role exists

               // Seed an initial administrator user
                logger.LogInformation("Seeding admin user.");
                var adminEmail = "admin@codehub.com";

                // Only create the admin user if it does not already exist
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {

                    // Create a new User object representing the admin
                    var adminUser = new User
                    {
                        FullName = "Code Hub",
                        UserName = adminEmail,
                        NormalizedUserName = adminEmail.ToUpper(),
                        Email = adminEmail,
                        NormalizedEmail = adminEmail.ToUpper(),
                        EmailConfirmed = true, // Email is confirmed for now until we do email varification
                        SecurityStamp = Guid.NewGuid().ToString()
                        
                    };

                    // Attempt to create the admin user with a default password
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");

                    if (result.Succeeded)
                    {
                         // Assign the Admin role to the newly created admin user
                        logger.LogInformation("Assigning Admin role to the admin user.");
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        // Log an error if creating the admin user failed
                        logger.LogError("Failed to create admin user");
                    }
                }
                else
                {
                    // Log information if the admin user already exists
                    logger.LogInformation("Failed to create admin user");
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during the seeding process
                logger.LogError(ex, "An error occurred while seeding data.");
                throw; // Rethrow exception to allow higher-level handling if necessary
            }
        }

        /// <summary>
        /// Ensures a role exists in the system; creates it if it does not exist.
        /// Throws an exception if role creation fails.
        /// </summary>
        /// <param name="roleManager">The RoleManager to manage Identity roles.</param>
        /// <param name="roleName">The name of the role to ensure exists.</param>
        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            // Check if the role already exists
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                // Attempt to create the role
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                // If creation failed, throw an exception with detailed error messages
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create {roleName} role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
       