using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.ViewComponents
{
    public class MainMenuViewComponent : ViewComponent
    {
        private readonly MenuService _menuService;

        public MainMenuViewComponent(MenuService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdValue = HttpContext.Session.GetInt32("UserId");
            if (!userIdValue.HasValue)
            {
                var claimUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(claimUserId, out var parsedUserId))
                {
                    HttpContext.Session.SetInt32("UserId", parsedUserId);
                    userIdValue = parsedUserId;
                }
            }

            if (userIdValue == null)
            {
                return View(new List<MenuItemViewModel>());
            }

            var menus = await _menuService.GetMenusForUserAsync(userIdValue.Value);
            var isSuperAdmin = string.Equals(HttpContext.User.Identity?.Name, "superadmin", StringComparison.OrdinalIgnoreCase);
            if (!isSuperAdmin)
            {
                menus = menus
                    .Where(menu => !menu.Url.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Keep nav focused: Home already acts as dashboard and "View Expenses" covers listing.
            menus = menus
                .Where(menu =>
                    !string.Equals(menu.Title, "Dashboard", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(menu.Title, "Expenses", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return View(menus);
        }
    }
}
