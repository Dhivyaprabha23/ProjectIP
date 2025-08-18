using InvestmentPortfolioManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManagementApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(SignInManager<ApplicationUser> signInManager,

                                 UserManager<ApplicationUser> userManager)

        {

            _signInManager = signInManager;

            _userManager = userManager;

        }



        [HttpGet]

        public IActionResult Login(string? role)

        {

            ViewBag.Role = role; // "Admin" or "User" 

            return View();

        }



        [HttpPost, ValidateAntiForgeryToken]

        public async Task<IActionResult> Login(string email, string password, string role)

        {

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))

            {

                ViewBag.Error = "All fields are required.";

                ViewBag.Role = role;

                return View();

            }



            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || !await _userManager.IsInRoleAsync(user, role))

            {

                ViewBag.Error = "Invalid credentials or role.";

                ViewBag.Role = role;

                return View();

            }



            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);

            if (!result.Succeeded)

            {

                ViewBag.Error = "Invalid credentials.";

                ViewBag.Role = role;

                return View();

            }



            return role == "Admin"

                ? RedirectToAction("Index", "AdminDashboard")

                : RedirectToAction("Index", "UserDashboard");

        }



        [HttpGet]

        public IActionResult Register(string? role)

        {

            ViewBag.Role = role;

            return View();

        }



        [HttpPost, ValidateAntiForgeryToken]

        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword, string role)

        {

            if (password != confirmPassword)

            {

                ViewBag.Error = "Passwords do not match.";

                ViewBag.Role = role;

                return View();

            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))

            {

                ViewBag.Error = "All fields are required.";

                ViewBag.Role = role;

                return View();

            }



            var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName };

            var result = await _userManager.CreateAsync(user, password);



            if (!result.Succeeded)

            {

                ViewBag.Error = string.Join("<br/>", result.Errors.Select(e => e.Description));

                ViewBag.Role = role;

                return View();

            }



            await _userManager.AddToRoleAsync(user, role);

            TempData["Msg"] = "Registration successful. Please login.";

            return RedirectToAction("Login", new { role });

        }



        [Authorize]

        public async Task<IActionResult> Logout()

        {

            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");

        }



        public IActionResult AccessDenied() => View();

    }

}