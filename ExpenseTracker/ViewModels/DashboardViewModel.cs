using System;
using System.Collections.Generic;

namespace ExpenseTracker.ViewModels
{
    /// <summary>
    /// Aggregated information used to render the dashboard page.
    /// Contains totals, category breakdown and recent entries; admin pages include per-user summaries.
    /// </summary>
    public class DashboardViewModel
    {
        public int TotalExpenses { get; set; }

        public decimal TotalAmount { get; set; }

        public List<CategorySummaryViewModel> Categories { get; set; } = new();

        public List<RecentExpenseViewModel> RecentExpenses { get; set; } = new();

        // Admin-only fields
        public List<UserExpenseViewModel> UserExpenses { get; set; } = new();

        public List<AdminRecentExpenseViewModel> AdminRecentExpenses { get; set; } = new();
    }

    /// <summary>
    /// Category summary for dashboard (category name, count and total amount).
    /// </summary>
    public class CategorySummaryViewModel
    {
        public string Category { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Recent expense row displayed in summary lists.
    /// </summary>
    public class RecentExpenseViewModel
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Per-user aggregation used on admin dashboards.
    /// </summary>
    public class UserExpenseViewModel
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public int TotalExpenses { get; set; }

        public decimal TotalAmount { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsSuperAdmin { get; set; }
    }

    /// <summary>
    /// Admin view of recent expenses including the originating username and flags.
    /// </summary>
    public class AdminRecentExpenseViewModel
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Username { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }

        public bool IsSuperAdmin { get; set; }
    }
}
