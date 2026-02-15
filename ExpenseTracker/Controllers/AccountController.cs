using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Controllers
{
    public class AccountController : Controller
    {
        private const string DefaultRoleName = "User";
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == model.Username);

            if (usernameExists)
            {
                ModelState.AddModelError(nameof(model.Username), "Username is already taken.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = PasswordHasher.HashPassword(model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var defaultRole = await EnsureRoleAsync(DefaultRoleName);

            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id
            });

            await _context.SaveChangesAsync();

            await EnsureDefaultMenusAsync(user.Id);

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            // Block superadmin from logging in via regular login page
            if (user.Username == "superadmin")
            {
                ModelState.AddModelError(string.Empty, "Superadmin login is restricted. Please use the admin portal.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            foreach (var role in user.UserRoles.Select(ur => ur.Role?.RoleName).Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                claims.Add(new Claim(ClaimTypes.Role, role!));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false, // Cookie expires when browser closes
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(120)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Clear cache to prevent back button from showing cached authenticated pages
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult NotAuthorized()
        {
            return View();
        }

        private async Task<Role> EnsureRoleAsync(string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role != null)
            {
                return role;
            }

            role = new Role { RoleName = roleName };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        private async Task EnsureDefaultMenusAsync(int userId)
        {
            var menus = await _context.Menus.ToListAsync();
            if (menus.Count == 0)
            {
                menus = new List<Menu>
                {
                    new Menu { Title = "Dashboard", Url = "/Dashboard" },
                    new Menu { Title = "Expenses", Url = "/Expense" }
                };

                _context.Menus.AddRange(menus);
                await _context.SaveChangesAsync();
            }

            var existingMenuIds = await _context.UserMenus
                .Where(um => um.UserId == userId)
                .Select(um => um.MenuId)
                .ToListAsync();

            var missingMenus = menus
                .Where(menu => !existingMenuIds.Contains(menu.Id))
                .Select(menu => new UserMenu
                {
                    UserId = userId,
                    MenuId = menu.Id
                })
                .ToList();

            if (missingMenus.Count > 0)
            {
                _context.UserMenus.AddRange(missingMenus);
                await _context.SaveChangesAsync();
            }
        }
    }
}
