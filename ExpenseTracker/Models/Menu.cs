using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Menu
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Url { get; set; } = string.Empty;

        public ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
    }
}
