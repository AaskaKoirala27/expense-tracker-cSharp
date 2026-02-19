using System.Collections.Generic;

namespace ExpenseTracker.ViewModels
{
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
