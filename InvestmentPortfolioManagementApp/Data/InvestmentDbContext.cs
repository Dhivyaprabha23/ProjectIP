using InvestmentPortfolioManagementApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManagementApp.Data
{
    public class InvestmentDbContext : IdentityDbContext<ApplicationUser>
    {
        public InvestmentDbContext(DbContextOptions<InvestmentDbContext> options) : base(options) { }

        public DbSet<User> UsersLocal { get; set; } = null!;
        public DbSet<Portfolio> Portfolios { get; set; } = null!;
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<Risk> Risks { get; set; } = null!;
        public DbSet<Performance> Performances { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Map User local entity to table name "User"
            builder.Entity<User>().ToTable("User");

            builder.Entity<Portfolio>()
                .HasOne(p => p.User)
                .WithMany(u => u.Portfolios)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Asset>()
                .HasOne(a => a.Portfolio)
                .WithMany(p => p.Assets)
                .HasForeignKey(a => a.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Performance>()
                .HasOne(perf => perf.Asset)
                .WithMany(a => a.Performances)
                .HasForeignKey(perf => perf.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Risk>()
                .HasOne(r => r.Portfolio)
                .WithMany(p => p.Risks)
                .HasForeignKey(r => r.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
