namespace ExpenseTracker.Models
{
    
    /// Join entity representing the assignment of a `Menu` to a `User`.
    /// Used to determine which navigation items are visible to a given user.
    
    public class UserMenu
    {
        
        /// Primary key for the UserMenu record.
        
        public int Id { get; set; }

        
        /// Foreign key to the `User` who has this menu assigned.
        
        public int UserId { get; set; }

        
        /// Foreign key to the `Menu` entry.
        
        public int MenuId { get; set; }

        
        /// Navigation property to the related `User`.
        
        public User? User { get; set; }

        
        /// Navigation property to the related `Menu`.
        
        public Menu? Menu { get; set; }
    }
}
