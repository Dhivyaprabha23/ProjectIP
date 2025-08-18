using InvestmentPortfolioManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManagementApp.Controllers
{
    [Authorize(Roles = "Investor")]
    public class InvestorDashboardController : Controller
    {
            private readonly UserManager<ApplicationUser> _userManager;
            public InvestorDashboardController(UserManager<ApplicationUser> userManager)
            {
                _userManager = userManager;
            }
            public async Task<IActionResult> Index()
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.UserName = (user != null && !string.IsNullOrWhiteSpace(user.FullName)) ? user.FullName : (User.Identity?.Name ?? "User");
                return View();
            }
        }

    }

