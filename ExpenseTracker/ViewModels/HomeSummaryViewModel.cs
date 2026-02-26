using System.Collections.Generic;

namespace ExpenseTracker.ViewModels
{
    /// View model used to render the home/index page summary information.
    /// Includes total counts, totals, recent expenses and admin-specific fields when applicable.
   
    public class HomeSummaryViewModel
    {
        public int TotalExpenses { get; set; }

        public decimal TotalAmount { get; set; }

        public List<RecentExpenseViewModel> RecentExpenses { get; set; } = new();

        // User monthly expenses
        public List<MonthlyExpenseViewModel> MonthlyExpenses { get; set; } = new();

        // Admin-only fields
        public List<UserExpenseViewModel> UserExpenses { get; set; } = new();

        public List<AdminRecentExpenseViewModel> AdminRecentExpenses { get; set; } = new();
    }
}
