namespace ExpenseTracker.Models
{
    
    /// Join entity representing the many-to-many relationship between users and roles.
    /// Stores foreign keys to `User` and `Role` and optional navigation properties.
    
    
    public class UserRole
    {
        
        /// Primary key for the UserRole record.
        
        
        public int Id { get; set; }

        
        /// Foreign key to the `User` entity.
        
        
        public int UserId { get; set; }

        
        /// Foreign key to the `Role` entity.
        
        
        public int RoleId { get; set; }

        
        /// Navigation property to the related `User`.
        
        
        public User? User { get; set; }

        
        /// Navigation property to the related `Role`.
        
        
        public Role? Role { get; set; }
    }
}
