using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NashunumaApp.Infrastructure.Data;
using NashunumaApp.Infrastructure.Identity;

namespace NashunumaApp.Infrastructure.Services
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseInitializer");


            try
            {
                 var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                logger.LogInformation(" Checking database status...");

                // ============================================================
                // STEP 1: CHECK IF DATABASE EXISTS AND CREATE IF NOT
                // ============================================================
                var databaseExists = await context.Database.CanConnectAsync();

                if (!databaseExists)
                {
                    logger.LogInformation(" Database does not exist. Creating database...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation(" Database created and migrations applied!");
                }
                else
                {
                    // ============================================================
                    // STEP 2: CHECK FOR PENDING MIGRATIONS
                    // ============================================================
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    var pendingMigrationsList = pendingMigrations.ToList();

                    if (pendingMigrationsList.Any())
                    {
                        logger.LogInformation($" Found {pendingMigrationsList.Count} pending migrations:");
                        foreach (var migration in pendingMigrationsList)
                        {
                            logger.LogInformation($"   - {migration}");
                        }

                        logger.LogInformation(" Applying pending migrations...");
                        await context.Database.MigrateAsync();
                        logger.LogInformation(" All migrations applied successfully!");
                    }
                    else
                    {
                        logger.LogInformation(" No pending migrations. Database is up to date!");
                    }
                }

                // ============================================================
                // STEP 3: CHECK IF SEEDING IS NEEDED (FIRST TIME ONLY)
                // ============================================================
                // Check if any user exists in the database
                var hasUsers = await userManager.Users.AnyAsync();

                if (!hasUsers)
                {
                    logger.LogInformation(" No users found. This appears to be the first run. Seeding initial data...");

                    // Seed roles (only if not already seeded)
                    await SeedRolesAsync(roleManager, logger);

                    // Seed admin user
                    await SeedAdminUserAsync(userManager, logger);

                    logger.LogInformation(" Initial data seeded successfully!");
                }
                else
                {
                    logger.LogInformation(" Users already exist. Skipping initial seed.");
                }

                logger.LogInformation(" Database initialization completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, " An error occurred while initializing the database");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            try
            {
                string[] roleNames = { "Admin", "User", "Manager" };
                bool rolesCreated = false;

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (result.Succeeded)
                        {
                            logger.LogInformation($" Role '{roleName}' created successfully!");
                            rolesCreated = true;
                        }
                        else
                        {
                            logger.LogError($" Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        logger.LogInformation($" Role '{roleName}' already exists.");
                    }
                }

                if (!rolesCreated)
                {
                    logger.LogInformation(" All roles already exist.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, " Error seeding roles");
                throw;
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            try
            {
                // Default admin credentials
                const string adminEmail = "bispadmin@gmail.com";
                const string adminPassword = "Abc@1234";
                const string adminFirstName = "Admin";
                const string adminLastName = "User";

                // Check if admin user exists
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    logger.LogInformation($" Creating admin user: {adminEmail}");

                    var user = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = adminFirstName,
                        LastName = adminLastName,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await userManager.CreateAsync(user, adminPassword);

                    if (result.Succeeded)
                    {
                        logger.LogInformation(" Admin user created successfully!");

                        // Assign Admin role
                        await userManager.AddToRoleAsync(user, "Admin");
                        logger.LogInformation(" Admin role assigned to admin user!");

                        // Assign User role
                        await userManager.AddToRoleAsync(user, "User");
                        logger.LogInformation(" User role assigned to admin user!");

                        logger.LogInformation($" Admin Credentials:");
                        logger.LogInformation($"   Email: {adminEmail}");
                        logger.LogInformation($"   Password: {adminPassword}");
                        logger.LogWarning("     Please change the password after first login!");
                    }
                    else
                    {
                        logger.LogError($" Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($" Admin user '{adminEmail}' already exists.");

                    // Ensure admin user has Admin role
                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation(" Admin role added to existing admin user!");
                    }

                    // Ensure admin user has User role
                    if (!await userManager.IsInRoleAsync(adminUser, "User"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "User");
                        logger.LogInformation(" User role added to existing admin user!");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, " Error seeding admin user");
                throw;
            }
        }
    }
}