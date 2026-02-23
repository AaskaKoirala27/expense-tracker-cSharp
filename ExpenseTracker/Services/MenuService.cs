/*
 * File: Services/MenuService.cs
 * Purpose: Provides menu retrieval utilities scoped to a specific user.
 * Responsibilities:
 *  - Query `UserMenu` assignments and return view models used by the UI to render a user's menu
 */

using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Services
{
    /// Service that fetches menu items assigned to a specific user and maps them to `MenuItemViewModel`.
    public class MenuService
    {
        private readonly AppDbContext _context;

        public MenuService(AppDbContext context)
        {
            _context = context;
        }


        /// Returns a list of `MenuItemViewModel` entries for the provided user id.

        /// <param name="userId">User id to fetch menus for.</param>
        /// <returns>List of menu items visible to the user.</returns>
        public async Task<List<MenuItemViewModel>> GetMenusForUserAsync(int userId)
        {
            return await _context.UserMenus
                .AsNoTracking()
                .Where(um => um.UserId == userId)
                .Include(um => um.Menu)
                .OrderBy(um => um.Menu!.Title)
                .Select(um => new MenuItemViewModel
                {
                    Title = um.Menu!.Title,
                    Url = um.Menu!.Url
                })
                .ToListAsync();
        }
    }
}
