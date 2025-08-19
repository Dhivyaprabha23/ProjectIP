using InvestmentPortfolioManagementApp.Data;
using InvestmentPortfolioManagementApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManagementApp.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly InvestmentDbContext _dbContext;

        public UserService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           InvestmentDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        public async Task<IdentityResult> RegisterUserAsync(string fullName, string email, string password, string role)
        {
            var appUser = new ApplicationUser { UserName = email, Email = email, FullName = fullName };
            var result = await _userManager.CreateAsync(appUser, password);
            if (!result.Succeeded) return result;
            await _userManager.AddToRoleAsync(appUser, role);

            await EnsureDomainUserAsync(email, role);
            return result;
        }

        public Task<SignInResult> LoginUserAsync(string email, string password)
        {
            return _signInManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);
        }

        public async Task<bool> UpdateUserProfileAsync(string identityUserId, string? fullName, string? email)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == identityUserId);
            if (user == null) return false;
            if (!string.IsNullOrWhiteSpace(fullName)) user.FullName = fullName;
            if (!string.IsNullOrWhiteSpace(email)) { user.Email = email; user.UserName = email; }
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<User> EnsureDomainUserAsync(string email, string role)
        {
            var existing = await _dbContext.UsersLocal.FirstOrDefaultAsync(u => u.Email == email);
            if (existing != null) return existing;
            var domainUser = new User
            {
                Username = email,
                Email = email,
                Password = string.Empty,
                Role = role
            };
            _dbContext.UsersLocal.Add(domainUser);
            await _dbContext.SaveChangesAsync();
            return domainUser;
        }
    }
}

