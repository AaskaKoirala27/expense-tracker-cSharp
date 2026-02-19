using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Services
{
    public class HomeSummaryService
    {
        private readonly AppDbContext _context;

        public HomeSummaryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HomeSummaryViewModel> GetSummaryAsync(int? userId, bool isAdmin)
        {
            if (!isAdmin && userId == null)
            {
                return new HomeSummaryViewModel();
            }

            var expensesQuery = _context.Expenses.AsNoTracking().AsQueryable();

            if (!isAdmin)
            {
                expensesQuery = expensesQuery.Where(e => e.UserId == userId);
            }

            var totalExpenses = await expensesQuery.CountAsync();
            var totalAmount = await expensesQuery.SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var recentExpenses = await expensesQuery
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .Take(5)
                .Select(e => new RecentExpenseViewModel
                {
                    Id = e.Id,
                    Date = e.Date,
                    Description = e.Description,
                    Category = e.Category,
                    Amount = e.Amount
                })
                .ToListAsync();

            var viewModel = new HomeSummaryViewModel
            {
                TotalExpenses = totalExpenses,
                TotalAmount = totalAmount,
                RecentExpenses = recentExpenses
            };

            // Get monthly expenses for both user and admin
            if (!isAdmin && userId.HasValue)
            {
                var userMonthlyExpenses = await _context.Expenses
                    .AsNoTracking()
                    .Where(e => e.UserId == userId)
                    .ToListAsync();

                viewModel.MonthlyExpenses = userMonthlyExpenses
                    .GroupBy(e => new { e.Date.Year, e.Date.Month })
                    .Select(g => new MonthlyExpenseViewModel
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        TotalAmount = g.Sum(e => e.Amount),
                        ExpenseCount = g.Count()
                    })
                    .OrderBy(m => m.Month)
                    .ToList();
            }

            // Admin-only features
            if (isAdmin)
            {
                // Get per-user expense totals
                var allExpensesQuery = _context.Expenses
                    .AsNoTracking()
                    .Include(e => e.User)
                    .ThenInclude(u => u!.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .AsQueryable();

                viewModel.UserExpenses = await allExpensesQuery
                    .GroupBy(e => new { e.UserId, Username = e.User!.Username })
                    .Select(group => new UserExpenseViewModel
                    {
                        UserId = group.Key.UserId ?? 0,
                        Username = group.Key.Username,
                        TotalExpenses = group.Count(),
                        TotalAmount = group.Sum(e => e.Amount),
                        IsAdmin = group.Any(e => e.User!.UserRoles.Any(ur => ur.Role != null && ur.Role.RoleName == "Admin")),
                        IsSuperAdmin = group.Key.Username == "superadmin"
                    })
                    .OrderByDescending(ue => ue.TotalAmount)
                    .ToListAsync();

                // Get recent expenses from all users with usernames
                viewModel.AdminRecentExpenses = await _context.Expenses
                    .AsNoTracking()
                    .Include(e => e.User)
                    .ThenInclude(u => u!.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .OrderByDescending(e => e.Date)
                    .ThenByDescending(e => e.Id)
                    .Take(5)
                    .Select(e => new AdminRecentExpenseViewModel
                    {
                        Id = e.Id,
                        Date = e.Date,
                        Description = e.Description,
                        Category = e.Category,
                        Amount = e.Amount,
                        Username = e.User!.Username,
                        IsAdmin = e.User!.UserRoles.Any(ur => ur.Role != null && ur.Role.RoleName == "Admin"),
                        IsSuperAdmin = e.User!.Username == "superadmin"
                    })
                    .ToListAsync();
            }

            return viewModel;
        }
    }
}
