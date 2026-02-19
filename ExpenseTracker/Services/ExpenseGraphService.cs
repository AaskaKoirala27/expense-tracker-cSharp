using ExpenseTracker.Data;
using ExpenseTracker.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class ExpenseGraphService
    {
        private readonly AppDbContext _context;

        public ExpenseGraphService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ExpenseGraphViewModel> GetExpenseGraphDataAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            // Validate dates
            if (startDate == null)
                startDate = DateTime.Now.AddMonths(-6);
            
            if (endDate == null)
                endDate = DateTime.Now;

            // Prevent future dates
            if (startDate > DateTime.Now)
                startDate = DateTime.Now.AddMonths(-6);
            
            if (endDate > DateTime.Now)
                endDate = DateTime.Now;

            // Ensure start date is before end date
            if (startDate > endDate)
            {
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            var expenses = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            // Group by date
            var dailyExpenses = expenses
                .GroupBy(e => e.Date.Date)
                .Select(g => new DailyExpenseViewModel
                {
                    Date = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Group by category
            var categoryExpenses = expenses
                .GroupBy(e => e.Category)
                .Select(g => new CategoryExpenseViewModel
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            var totalAmount = expenses.Sum(e => e.Amount);
            var totalCount = expenses.Count;

            return new ExpenseGraphViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                DailyExpenses = dailyExpenses,
                CategoryExpenses = categoryExpenses,
                TotalAmount = totalAmount,
                TotalCount = totalCount,
                AverageDaily = dailyExpenses.Count > 0 ? dailyExpenses.Average(d => d.Amount) : 0
            };
        }

        public async Task<ExpenseGraphViewModel> GetSystemExpenseGraphDataAsync(DateTime? startDate, DateTime? endDate)
        {
            // Validate dates
            if (startDate == null)
                startDate = DateTime.Now.AddMonths(-6);
            
            if (endDate == null)
                endDate = DateTime.Now;

            // Prevent future dates
            if (startDate > DateTime.Now)
                startDate = DateTime.Now.AddMonths(-6);
            
            if (endDate > DateTime.Now)
                endDate = DateTime.Now;

            // Ensure start date is before end date
            if (startDate > endDate)
            {
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            // Get all expenses from all users
            var expenses = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            // Group by date
            var dailyExpenses = expenses
                .GroupBy(e => e.Date.Date)
                .Select(g => new DailyExpenseViewModel
                {
                    Date = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Group by category
            var categoryExpenses = expenses
                .GroupBy(e => e.Category)
                .Select(g => new CategoryExpenseViewModel
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            var totalAmount = expenses.Sum(e => e.Amount);
            var totalCount = expenses.Count;

            return new ExpenseGraphViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                DailyExpenses = dailyExpenses,
                CategoryExpenses = categoryExpenses,
                TotalAmount = totalAmount,
                TotalCount = totalCount,
                AverageDaily = dailyExpenses.Count > 0 ? dailyExpenses.Average(d => d.Amount) : 0
            };
        }
    }
}
