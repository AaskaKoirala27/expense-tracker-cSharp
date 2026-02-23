using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{

    /// Represents a navigation menu entry (title + URL) used to build the application's menu.
    public class Menu
    {

        /// Primary key for the menu record.
        public int Id { get; set; }

        /// Display title for the menu item.

        [Required]
        public string Title { get; set; } = string.Empty;

        /// URL or path the menu item links to.

        [Required]
        public string Url { get; set; } = string.Empty;

        /// Navigation collection for users who have this menu assigned.

        public ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
    }
}
