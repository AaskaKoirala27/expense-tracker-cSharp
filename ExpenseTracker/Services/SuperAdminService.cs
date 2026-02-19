using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Services
{
    public class SuperAdminService
    {
        private readonly AppDbContext _context;

        public SuperAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SuperAdminDashboardViewModel> GetDashboardDataAsync()
        {
            var totalUsers = await _context.Users
                .Where(u => u.Username != "superadmin")
                .CountAsync();

            var totalExpenses = await _context.Expenses.CountAsync();

            var monthlyExpenses = await _context.Expenses
                .AsNoTracking()
                .ToListAsync(); // Bring data into memory first

            var monthlyData = monthlyExpenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new MonthlyExpenseViewModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalAmount = g.Sum(e => e.Amount),
                    ExpenseCount = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            return new SuperAdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalExpenses = totalExpenses,
                MonthlyExpenses = monthlyData
            };
        }

        public async Task<List<SuperAdminUserViewModel>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.Username != "superadmin")
                .AsNoTracking()
                .Select(u => new SuperAdminUserViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    RegistrationDate = new DateTime(2026, 2, 1), // You can add CreatedAt to User model later
                    IsActive = true, // You can add IsActive to User model later
                    TotalExpenses = u.Expenses.Count,
                    TotalAmount = u.Expenses.Sum(e => e.Amount)
                })
                .ToListAsync();

            return users;
        }

        public async Task<List<SuperAdminExpenseViewModel>> GetAllExpensesAsync()
        {
            var expenses = await _context.Expenses
                .AsNoTracking()
                .Include(e => e.User)
                .OrderByDescending(e => e.Date)
                .Select(e => new SuperAdminExpenseViewModel
                {
                    Id = e.Id,
                    Username = e.User!.Username,
                    Category = e.Category,
                    Amount = e.Amount,
                    Date = e.Date,
                    Description = e.Description
                })
                .ToListAsync();

            return expenses;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Username == "superadmin")
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Username == "superadmin")
                return false;

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
