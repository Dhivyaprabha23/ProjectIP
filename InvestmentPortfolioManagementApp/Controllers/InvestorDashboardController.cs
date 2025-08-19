using InvestmentPortfolioManagementApp.Data;
using InvestmentPortfolioManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManagementApp.Controllers
{
    [Authorize(Roles = "Investor")]
    public class InvestorDashboardController : Controller
    {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly InvestmentDbContext _db;
            public InvestorDashboardController(UserManager<ApplicationUser> userManager, InvestmentDbContext db)
            {
                _userManager = userManager;
                _db = db;
            }
            public async Task<IActionResult> Index()
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.UserName = (user != null && !string.IsNullOrWhiteSpace(user.FullName)) ? user.FullName : (User.Identity?.Name ?? "User");
                // Load current investor domain record if exists
                var investor = await _db.UsersLocal.FirstOrDefaultAsync(u => u.Email == user!.Email && u.Role == Role.Investor);
                return View(investor);
            }

            [HttpGet]
            public async Task<IActionResult> EditInvestor()
            {
                var user = await _userManager.GetUserAsync(User);
                var investor = await _db.UsersLocal.FirstOrDefaultAsync(u => u.Email == user!.Email && u.Role == Role.Investor);
                if (investor == null)
                {
                    investor = new User { UserName = user!.FullName ?? user!.UserName ?? user!.Email!, Email = user!.Email!, Role = Role.Investor };
                }
                return View(investor);
            }

            [HttpPost, ValidateAntiForgeryToken]
            public async Task<IActionResult> EditInvestor(int userId, string userName, string email)
            {
                var currentIdentity = await _userManager.GetUserAsync(User);
                if (currentIdentity == null) return Unauthorized();

                var investor = userId == 0
                    ? await _db.UsersLocal.FirstOrDefaultAsync(u => u.Email == currentIdentity.Email && u.Role == Role.Investor)
                    : await _db.UsersLocal.FirstOrDefaultAsync(u => u.UserId == userId && u.Role == Role.Investor);

                if (investor == null)
                {
                    investor = new User { Role = Role.Investor };
                    _db.UsersLocal.Add(investor);
                }
                investor.UserName = userName;
                investor.Email = email;
                await _db.SaveChangesAsync();

                // Update identity fields
                currentIdentity.FullName = userName;
                currentIdentity.Email = email;
                currentIdentity.UserName = email;
                await _userManager.UpdateAsync(currentIdentity);

                TempData["Msg"] = "Profile updated.";
                return RedirectToAction(nameof(Index));
            }

            [HttpGet]
            public IActionResult CreateInvestor()
            {
                return View(new User { Role = Role.Investor });
            }

            [HttpPost, ValidateAntiForgeryToken]
            public async Task<IActionResult> CreateInvestor(string userName, string email, string password)
            {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "All fields are required.";
                    return View(new User { Role = Role.Investor, UserName = userName, Email = email });
                }

                // Only create local domain record; admin is responsible for creating identity users
                var exists = await _db.UsersLocal.AnyAsync(u => u.Email == email && u.Role == Role.Investor);
                if (exists)
                {
                    ViewBag.Error = "Investor with this email already exists.";
                    return View(new User { Role = Role.Investor, UserName = userName, Email = email });
                }

                var investor = new User { UserName = userName, Email = email, Password = password, Role = Role.Investor };
                _db.UsersLocal.Add(investor);
                await _db.SaveChangesAsync();
                TempData["Msg"] = "Investor created.";
                return RedirectToAction(nameof(Index));
            }
        }

    }

