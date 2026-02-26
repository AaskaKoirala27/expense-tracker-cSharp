using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.ViewModels
{
  
    /// View model used for the login form.

    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
