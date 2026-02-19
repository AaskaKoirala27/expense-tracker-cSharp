using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ExpenseTracker.Controllers
{
    [Authorize(Policy = "SuperAdminOnly")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminController : Controller
    {
        private const string SUPERADMIN_USERNAME = "superadmin";
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsSuperAdmin()
        {
            return User.Identity?.Name == SUPERADMIN_USERNAME;
        }

        // Superadmin Login Actions
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // If already logged in as superadmin, go to admin portal
                if (User.Identity.Name == SUPERADMIN_USERNAME)
                {
                    return RedirectToAction(nameof(Index));
                }
                // If logged in as regular user/admin, logout first
                return RedirectToAction("Logout", "Account");
            }

            return View(new LoginViewModel());
        }

        [AllowAnonymous]
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

            // Only allow superadmin to login through this page
            if (user.Username != SUPERADMIN_USERNAME)
            {
                ModelState.AddModelError(string.Empty, "This login page is for superadmin only. Please use the regular login page.");
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
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(120)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Index", "Home");
        }

        // Admin Portal Dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalAdmins = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role!.RoleName == "Admin"))
                .CountAsync();
            var totalMenus = await _context.Menus.CountAsync();
            var totalExpenses = await _context.Expenses.CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalAdmins = totalAdmins;
            ViewBag.TotalMenus = totalMenus;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.IsSuperAdmin = IsSuperAdmin();

            return View();
        }

        // Users Actions
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .ToListAsync();
            
            // For non-superadmin users, exclude superadmin from the list
            if (!IsSuperAdmin())
            {
                users = users.Where(u => u.Username != SUPERADMIN_USERNAME).ToList();
            }

            ViewBag.IsSuperAdmin = IsSuperAdmin();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            ViewBag.Roles = await _context.Roles.AsNoTracking().ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([Bind("Username,PasswordHash")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Users));
            }

            ViewBag.Roles = await _context.Roles.AsNoTracking().ToListAsync();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = await _context.Roles.AsNoTracking().ToListAsync();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, [Bind("Id,Username,PasswordHash")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction(nameof(Users));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.Roles = await _context.Roles.AsNoTracking().ToListAsync();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent non-superadmin users from deleting superadmin
            if (user.Username == SUPERADMIN_USERNAME && !IsSuperAdmin())
            {
                TempData["ErrorMessage"] = "Only the superadmin can delete the superadmin account.";
                return RedirectToAction(nameof(Users));
            }

            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Only superadmin can delete superadmin account
                if (user.Username == SUPERADMIN_USERNAME && !IsSuperAdmin())
                {
                    TempData["ErrorMessage"] = "Only the superadmin can delete the superadmin account.";
                    return RedirectToAction(nameof(Users));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User deleted successfully.";
            }

            return RedirectToAction(nameof(Users));
        }

        // CreateAdmin removed — superadmin handles users directly; no separate admin creation page.

        // User Role Management Actions
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Prevent non-superadmin users from modifying superadmin roles
            if (user.Username == SUPERADMIN_USERNAME && !IsSuperAdmin())
            {
                TempData["ErrorMessage"] = "Only the superadmin can modify the superadmin account roles.";
                return RedirectToAction(nameof(Users));
            }

            var allRoles = await _context.Roles.AsNoTracking().ToListAsync();
            ViewBag.AllRoles = allRoles;
            ViewBag.IsSuperAdmin = IsSuperAdmin();
            ViewBag.CurrentAdminRole = user.UserRoles.Any(ur => ur.Role?.RoleName == "Admin");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignAdminRole(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Prevent non-superadmin from assigning admin roles
            if (!IsSuperAdmin())
            {
                TempData["ErrorMessage"] = "Only the superadmin can assign admin roles.";
                return RedirectToAction(nameof(ManageUserRoles), new { id });
            }

            // Prevent modifying superadmin
            if (user.Username == SUPERADMIN_USERNAME)
            {
                TempData["ErrorMessage"] = "Cannot modify the superadmin account.";
                return RedirectToAction(nameof(ManageUserRoles), new { id });
            }

            // Check if user already has Admin role
            var hasAdminRole = user.UserRoles.Any(ur => ur.Role?.RoleName == "Admin");
            if (hasAdminRole)
            {
                TempData["WarningMessage"] = "This user already has the Admin role.";
                return RedirectToAction(nameof(ManageUserRoles), new { id });
            }

            // Get Admin role
            var adminRole = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleName == "Admin");

            if (adminRole == null)
            {
                TempData["ErrorMessage"] = "Admin role not found in database.";
                return RedirectToAction(nameof(ManageUserRoles), new { id });
            }

            // Add Admin role to user
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Admin role assigned to {user.Username} successfully.";
            return RedirectToAction(nameof(ManageUserRoles), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdminRole(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Prevent non-superadmin from removing admin roles
            if (!IsSuperAdmin())
            {
                TempData["ErrorMessage"] = "Only the superadmin can remove admin roles.";
                return RedirectToAction(nameof(ManageUserRoles), new { id });
            }

            // Prevent modifying superadmin
            if (user.Username == SUPERADMIN_USERNAME)
            {
                TempData["ErrorMessage"] = "Cannot modify the superadmin account.";
                return RedirectToAction(nameof(ManageUserRoles), new { id });
            }

            // Find and remove Admin role
            var adminUserRole = user.UserRoles.FirstOrDefault(ur => ur.Role?.RoleName == "Admin");
            if (adminUserRole == null)
            {
                TempData["WarningMessage"] = "This user does not have the Admin role.";
                return RedirectToAction(nameof(ManageUserRoles), new { id });
            }

            _context.UserRoles.Remove(adminUserRole);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Admin role removed from {user.Username} successfully.";
            return RedirectToAction(nameof(ManageUserRoles), new { id });
        }

        // Menus Actions
        [HttpGet]
        public async Task<IActionResult> Menus()
        {
            var menus = await _context.Menus.AsNoTracking().ToListAsync();
            return View(menus);
        }

        [HttpGet]
        public IActionResult CreateMenu()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenu([Bind("Title,Url")] Menu menu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(menu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Menu created successfully.";
                return RedirectToAction(nameof(Menus));
            }

            return View(menu);
        }

        [HttpGet]
        public async Task<IActionResult> EditMenu(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMenu(int id, [Bind("Id,Title,Url")] Menu menu)
        {
            if (id != menu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menu);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Menu updated successfully.";
                    return RedirectToAction(nameof(Menus));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuExists(menu.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(menu);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsMenu(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = await _context.Menus
                .Include(m => m.UserMenus)
                .ThenInclude(um => um.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteMenu(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == id);
            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        [HttpPost, ActionName("DeleteMenu")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenuConfirmed(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu != null)
            {
                _context.Menus.Remove(menu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Menu deleted successfully.";
            }

            return RedirectToAction(nameof(Menus));
        }

        // UserMenu Assignment Actions
        [HttpGet]
        public async Task<IActionResult> AssignMenus()
        {
            var roles = await _context.Roles.AsNoTracking().ToListAsync();
            var menus = await _context.Menus.AsNoTracking().ToListAsync();

            ViewBag.Roles = roles;
            ViewBag.Menus = menus;

            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMenuAssignments(int roleId, int[] selectedMenuIds)
        {
            try
            {
                // Get all current UserMenus for users with this role
                var usersWithRole = await _context.Users
                    .Include(u => u.UserRoles)
                    .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
                    .Select(u => u.Id)
                    .ToListAsync();

                if (usersWithRole.Count == 0)
                {
                    TempData["WarningMessage"] = "No users found with this role.";
                    return RedirectToAction(nameof(AssignMenus));
                }

                // Remove existing menu assignments for these users
                var existingAssignments = await _context.UserMenus
                    .Where(um => usersWithRole.Contains(um.UserId))
                    .ToListAsync();

                _context.UserMenus.RemoveRange(existingAssignments);
                await _context.SaveChangesAsync();

                // Add new menu assignments
                var newAssignments = new List<UserMenu>();
                foreach (var userId in usersWithRole)
                {
                    foreach (var menuId in selectedMenuIds)
                    {
                        // Prevent duplicates
                        if (!newAssignments.Any(um => um.UserId == userId && um.MenuId == menuId))
                        {
                            newAssignments.Add(new UserMenu
                            {
                                UserId = userId,
                                MenuId = menuId
                            });
                        }
                    }
                }

                if (newAssignments.Count > 0)
                {
                    _context.UserMenus.AddRange(newAssignments);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Menu assignments updated successfully for {usersWithRole.Count} user(s).";
                return RedirectToAction(nameof(AssignMenus));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating menu assignments: {ex.Message}";
                return RedirectToAction(nameof(AssignMenus));
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.Id == id);
        }

        // SuperAdmin - Manage Users
        [Authorize(Policy = "SuperAdminOnly")]
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var superAdminService = HttpContext.RequestServices.GetRequiredService<SuperAdminService>();
            var users = await superAdminService.GetAllUsersAsync();
            return View(users);
        }

        // SuperAdmin - Delete User
        [Authorize(Policy = "SuperAdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserSuper(int id)
        {
            var superAdminService = HttpContext.RequestServices.GetRequiredService<SuperAdminService>();
            var result = await superAdminService.DeleteUserAsync(id);
            
            if (result)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        // SuperAdmin - View All Expenses
        [Authorize(Policy = "SuperAdminOnly")]
        [HttpGet]
        public async Task<IActionResult> ViewAllExpenses()
        {
            var superAdminService = HttpContext.RequestServices.GetRequiredService<SuperAdminService>();
            var expenses = await superAdminService.GetAllExpensesAsync();
            return View(expenses);
        }    }
}