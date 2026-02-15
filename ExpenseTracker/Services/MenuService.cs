using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Services
{
    public class MenuService
    {
        private readonly AppDbContext _context;

        public MenuService(AppDbContext context)
        {
            _context = context;
        }

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
