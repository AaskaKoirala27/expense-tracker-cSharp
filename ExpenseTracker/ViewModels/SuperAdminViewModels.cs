namespace ExpenseTracker.ViewModels
{
    public class SuperAdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalExpenses { get; set; }
        public List<MonthlyExpenseViewModel> MonthlyExpenses { get; set; } = new();
    }

    public class MonthlyExpenseViewModel
    {
        public DateTime Month { get; set; }
        public decimal TotalAmount { get; set; }
        public int ExpenseCount { get; set; }
    }

    public class SuperAdminUserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalExpenses { get; set; }
        public decimal TotalAmount { get; set; }
    }

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
