namespace ExpenseTracker.Models
{
    /// <summary>
    /// Join entity representing the many-to-many relationship between users and roles.
    /// Stores foreign keys to `User` and `Role` and optional navigation properties.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// Primary key for the UserRole record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the `User` entity.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Foreign key to the `Role` entity.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Navigation property to the related `User`.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Navigation property to the related `Role`.
        /// </summary>
        public Role? Role { get; set; }
    }
}
