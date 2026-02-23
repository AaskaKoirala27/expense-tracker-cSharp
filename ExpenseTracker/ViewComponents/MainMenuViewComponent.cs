/*
 * File: ViewComponents/MainMenuViewComponent.cs
 * Purpose: Renders the main navigation menu for the currently logged-in user.
 * Responsibilities:
 *  - Use `MenuService` to fetch assigned `MenuItemViewModel` entries for the session user
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.ViewComponents
{
    /// <summary>
    /// View component that builds the main application menu based on the current user's menu assignments.
    /// Returns an empty list for anonymous sessions.
    /// </summary>
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

            if (userIdValue == null)
            {
                return View(new List<MenuItemViewModel>());
            }

            var menus = await _menuService.GetMenusForUserAsync(userIdValue.Value);
            return View(menus);
        }
    }
}
