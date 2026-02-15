using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        public ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
    }
}
