using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    /// <summary>
    /// Represents a navigation menu entry (title + URL) used to build the application's menu.
    /// </summary>
    public class Menu
    {
        /// <summary>
        /// Primary key for the menu record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display title for the menu item.
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// URL or path the menu item links to.
        /// </summary>
        [Required]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Navigation collection for users who have this menu assigned.
        /// </summary>
        public ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
    }
}
