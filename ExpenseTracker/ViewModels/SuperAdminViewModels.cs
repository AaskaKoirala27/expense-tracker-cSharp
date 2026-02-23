namespace ExpenseTracker.ViewModels
{
    /// <summary>
    /// View model for the superadmin dashboard providing system-wide metrics.
    /// </summary>
    public class SuperAdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalExpenses { get; set; }
        public List<MonthlyExpenseViewModel> MonthlyExpenses { get; set; } = new();
    }

    /// <summary>
    /// Monthly aggregated expense row used in the superadmin dashboard.
    /// </summary>
    public class MonthlyExpenseViewModel
    {
        public DateTime Month { get; set; }
        public decimal TotalAmount { get; set; }
        public int ExpenseCount { get; set; }
    }

    /// <summary>
    /// Representation of a user for the superadmin listing page.
    /// </summary>
    public class SuperAdminUserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalExpenses { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Representation of an expense for superadmin listing.
    /// </summary>
    public class SuperAdminExpenseViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
