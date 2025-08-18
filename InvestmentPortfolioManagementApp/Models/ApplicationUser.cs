using Microsoft.AspNetCore.Identity;

namespace InvestmentPortfolioManagementApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
