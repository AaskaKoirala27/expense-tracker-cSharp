namespace ExpenseTracker.ViewModels
{
    
    /// Simple view model representing a navigation item (title + url) used by the main menu.
   
    public class MenuItemViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}
