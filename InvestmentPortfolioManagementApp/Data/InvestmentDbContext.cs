using InvestmentPortfolioManagementApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManagementApp.Data
{
    public class InvestmentDbContext : IdentityDbContext<ApplicationUser>
    {
        public InvestmentDbContext(DbContextOptions<InvestmentDbContext> options) : base(options) { }
    }
}
