using System;
using System.Collections.Generic;

namespace ExpenseTracker.ViewModels
{
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

    public class CategorySummaryViewModel
    {
        public string Category { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal TotalAmount { get; set; }
    }

    public class RecentExpenseViewModel
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }

    public class UserExpenseViewModel
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public int TotalExpenses { get; set; }

        public decimal TotalAmount { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsSuperAdmin { get; set; }
    }

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
