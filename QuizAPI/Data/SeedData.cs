using Microsoft.AspNetCore.Identity;
using QuizAPI.Models;

namespace QuizAPI.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Resolve the required services from the DI container
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // 2. Create an admin user if not found
            var adminEmail = "AdminEmail";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                // Provide a default password for the admin account
                var createResult = await userManager.CreateAsync(adminUser, "AdminPassword");
                if (createResult.Succeeded)
                    // Assign the Admin role to the new user
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
