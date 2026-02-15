using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();

            var roles = await context.Roles.ToListAsync();
            if (roles.Count == 0)
            {
                roles = new List<Role>
                {
                    new Role { RoleName = "User" },
                    new Role { RoleName = "Admin" }
                };

                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
            }

            // Create superadmin user if it doesn't exist
            var superadminExists = await context.Users.AnyAsync(u => u.Username == "superadmin");
            if (!superadminExists)
            {
                var superadmin = new User
                {
                    Username = "superadmin",
                    PasswordHash = PasswordHasher.HashPassword("SuperSecret123")
                };
                context.Users.Add(superadmin);
                await context.SaveChangesAsync();

                // Assign superadmin to BOTH Admin and User roles (superadmin has all permissions)
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
                var userRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                
                if (adminRole != null)
                {
                    var superadminAdminRoleExists = await context.UserRoles
                        .AnyAsync(ur => ur.UserId == superadmin.Id && ur.RoleId == adminRole.Id);
                    
                    if (!superadminAdminRoleExists)
                    {
                        context.UserRoles.Add(new UserRole
                        {
                            UserId = superadmin.Id,
                            RoleId = adminRole.Id
                        });
                    }
                }
                
                if (userRole != null)
                {
                    var superadminUserRoleExists = await context.UserRoles
                        .AnyAsync(ur => ur.UserId == superadmin.Id && ur.RoleId == userRole.Id);
                    
                    if (!superadminUserRoleExists)
                    {
                        context.UserRoles.Add(new UserRole
                        {
                            UserId = superadmin.Id,
                            RoleId = userRole.Id
                        });
                    }
                }
                
                await context.SaveChangesAsync();
                
                // Assign ALL menus to superadmin
                var superadminMenusOnCreation = await context.Menus.ToListAsync();
                foreach (var menu in superadminMenusOnCreation)
                {
                    var menuExists = await context.UserMenus
                        .AnyAsync(um => um.UserId == superadmin.Id && um.MenuId == menu.Id);
                    
                    if (!menuExists)
                    {
                        context.UserMenus.Add(new UserMenu
                        {
                            UserId = superadmin.Id,
                            MenuId = menu.Id
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            // Seed menus with duplicate prevention
            var existingMenus = await context.Menus.AsNoTracking().ToListAsync();
            var menusToAdd = new List<Menu>();

            var defaultMenus = new List<(string Title, string Url)>
            {
                ("Dashboard", "/Dashboard/Index"),
                ("Add Expense", "/Expense/Create"),
                ("View Expenses", "/Expense/Index"),
                ("Manage Users", "/Admin/Users"),
                ("Manage Menus", "/Admin/Menus")
            };

            foreach (var (title, url) in defaultMenus)
            {
                if (!existingMenus.Any(m => m.Title == title))
                {
                    menusToAdd.Add(new Menu { Title = title, Url = url });
                }
            }

            if (menusToAdd.Count > 0)
            {
                context.Menus.AddRange(menusToAdd);
                await context.SaveChangesAsync();
            }

            // Get all menus (newly added + existing)
            var allMenus = await context.Menus.AsNoTracking().ToListAsync();
            var menuMap = allMenus.ToDictionary(menu => menu.Title, menu => menu);

            var roleMenuMap = new Dictionary<string, string[]>
            {
                { "User", new[] { "Dashboard", "Add Expense", "View Expenses" } },
                { "Admin", new[] { "Dashboard", "View Expenses", "Manage Users", "Manage Menus" } }
            };

            // Seed UserMenus with duplicate prevention
            var users = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .ToListAsync();

            if (users.Count == 0)
            {
                return;
            }

            var existingUserMenus = await context.UserMenus.AsNoTracking().ToListAsync();
            var userMenusToAdd = new List<UserMenu>();

            foreach (var user in users)
            {
                var roleNames = user.UserRoles
                    .Select(ur => ur.Role?.RoleName)
                    .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                    .Distinct()
                    .ToList();

                var menuTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var roleName in roleNames)
                {
                    if (roleName != null && roleMenuMap.TryGetValue(roleName, out var titles))
                    {
                        foreach (var title in titles)
                        {
                            menuTitles.Add(title);
                        }
                    }
                }

                foreach (var title in menuTitles)
                {
                    if (menuMap.TryGetValue(title, out var menu))
                    {
                        // Check if this UserMenu already exists
                        if (!existingUserMenus.Any(um => um.UserId == user.Id && um.MenuId == menu.Id))
                        {
                            userMenusToAdd.Add(new UserMenu
                            {
                                UserId = user.Id,
                                MenuId = menu.Id
                            });
                        }
                    }
                }
            }

            if (userMenusToAdd.Count > 0)
            {
                context.UserMenus.AddRange(userMenusToAdd);
                await context.SaveChangesAsync();
            }

            // Ensure superadmin always has both User and Admin roles and ALL menus
            var existingSuperadmin = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == "superadmin");

            if (existingSuperadmin != null)
            {
                var adminRoleForUpdate = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
                var userRoleForUpdate = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");

                // Ensure Admin role
                if (adminRoleForUpdate != null && !existingSuperadmin.UserRoles.Any(ur => ur.RoleId == adminRoleForUpdate.Id))
                {
                    context.UserRoles.Add(new UserRole
                    {
                        UserId = existingSuperadmin.Id,
                        RoleId = adminRoleForUpdate.Id
                    });
                }

                // Ensure User role
                if (userRoleForUpdate != null && !existingSuperadmin.UserRoles.Any(ur => ur.RoleId == userRoleForUpdate.Id))
                {
                    context.UserRoles.Add(new UserRole
                    {
                        UserId = existingSuperadmin.Id,
                        RoleId = userRoleForUpdate.Id
                    });
                }

                await context.SaveChangesAsync();

                // Ensure superadmin has ALL menus
                var menusForSuperadmin = await context.Menus.ToListAsync();
                var superadminExistingMenus = await context.UserMenus
                    .Where(um => um.UserId == existingSuperadmin.Id)
                    .Select(um => um.MenuId)
                    .ToListAsync();

                foreach (var menuItem in menusForSuperadmin)
                {
                    if (!superadminExistingMenus.Contains(menuItem.Id))
                    {
                        context.UserMenus.Add(new UserMenu
                        {
                            UserId = existingSuperadmin.Id,
                            MenuId = menuItem.Id
                        });
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
