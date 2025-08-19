using InvestmentPortfolioManagementApp.Data;
using InvestmentPortfolioManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManagementApp.Controllers
{
    [Authorize(Roles = "Admin")]

    public class AdminDashboardController : Controller

    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly InvestmentDbContext _db;

        public AdminDashboardController(UserManager<ApplicationUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        InvestmentDbContext db)

        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;

        }

        public async Task<IActionResult> Index()

        {

            var current = await _userManager.GetUserAsync(User);
            ViewBag.UserName = (current != null && !string.IsNullOrWhiteSpace(current.FullName)) ? current.FullName : (User.Identity?.Name ?? "Admin");

            var investors = await _db.UsersLocal.Where(u => u.Role == Role.Investor).ToListAsync();
            return View(investors);

        }

        [HttpGet]
        public IActionResult CreateInvestor()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInvestor(string userName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            var emailExists = await _userManager.FindByEmailAsync(email);
            if (emailExists != null)
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }

            var appUser = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = userName };
            var createResult = await _userManager.CreateAsync(appUser, password);
            if (!createResult.Succeeded)
            {
                ViewBag.Error = string.Join("<br/>", createResult.Errors.Select(e => e.Description));
                return View();
            }
            if (!await _roleManager.RoleExistsAsync("Investor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Investor"));
            }
            await _userManager.AddToRoleAsync(appUser, "Investor");

            // Create domain user
            var domainUser = new User
            {
                UserName = userName,
                Email = email,
                Password = password,
                Role = Role.Investor
            };
            _db.UsersLocal.Add(domainUser);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "Investor created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditInvestor(int id)
        {
            var investor = await _db.UsersLocal.FirstOrDefaultAsync(u => u.UserId == id && u.Role == Role.Investor);
            if (investor == null) return NotFound();
            return View(investor);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInvestor(int userId, string userName, string email)
        {
            var investor = await _db.UsersLocal.FirstOrDefaultAsync(u => u.UserId == userId && u.Role == Role.Investor);
            if (investor == null) return NotFound();

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "User name and email are required.";
                return View(investor);
            }

            investor.UserName = userName;
            investor.Email = email;

            // Update identity user too
            var appUser = await _userManager.FindByEmailAsync(email) ?? await _userManager.Users.FirstOrDefaultAsync(u => u.Email == investor.Email);
            if (appUser != null)
            {
                appUser.Email = email;
                appUser.UserName = email;
                appUser.FullName = userName;
                await _userManager.UpdateAsync(appUser);
            }

            await _db.SaveChangesAsync();
            TempData["Msg"] = "Investor updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInvestor(int id)
        {
            var investor = await _db.UsersLocal.FirstOrDefaultAsync(u => u.UserId == id && u.Role == Role.Investor);
            if (investor == null) return NotFound();

            // delete identity user
            var identityUser = await _userManager.FindByEmailAsync(investor.Email);
            if (identityUser != null)
            {
                await _userManager.DeleteAsync(identityUser);
            }

            _db.UsersLocal.Remove(investor);
            await _db.SaveChangesAsync();
            TempData["Msg"] = "Investor deleted.";
            return RedirectToAction(nameof(Index));
        }

    }
}


