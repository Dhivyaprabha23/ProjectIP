using InvestmentPortfolioManagementApp.Models;
using Microsoft.AspNetCore.Identity;

namespace InvestmentPortfolioManagementApp.Data
{
    public class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var roleName in new[] { "Admin", "Investor" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }
            // Admin 
            var adminEmail = "admin@portfolio.com";
            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Admin"
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            // User 
            var userEmail = "investor@portfolio.com";
            if (await userManager.FindByEmailAsync(userEmail) is null)
            {
                var user = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    FullName = "Default User"                
                };
                await userManager.CreateAsync(user, "Investor@123");
                await userManager.AddToRoleAsync(user, "Investor");
            }
        }
    }
}