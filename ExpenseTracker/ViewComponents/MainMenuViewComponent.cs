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

            if (userIdValue == null)
            {
                return View(new List<MenuItemViewModel>());
            }

            var menus = await _menuService.GetMenusForUserAsync(userIdValue.Value);
            return View(menus);
        }
    }
}
