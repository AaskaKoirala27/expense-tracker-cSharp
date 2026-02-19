namespace ExpenseTracker.ViewModels
{
    public class ExpenseGraphViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DailyExpenseViewModel> DailyExpenses { get; set; } = new();
        public List<CategoryExpenseViewModel> CategoryExpenses { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public decimal AverageDaily { get; set; }
    }

    public class DailyExpenseViewModel
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class CategoryExpenseViewModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
}
