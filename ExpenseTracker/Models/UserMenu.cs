namespace ExpenseTracker.Models
{
    /// <summary>
    /// Join entity representing the assignment of a `Menu` to a `User`.
    /// Used to determine which navigation items are visible to a given user.
    /// </summary>
    public class UserMenu
    {
        /// <summary>
        /// Primary key for the UserMenu record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the `User` who has this menu assigned.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Foreign key to the `Menu` entry.
        /// </summary>
        public int MenuId { get; set; }

        /// <summary>
        /// Navigation property to the related `User`.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Navigation property to the related `Menu`.
        /// </summary>
        public Menu? Menu { get; set; }
    }
}
