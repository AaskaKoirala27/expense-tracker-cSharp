using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{

    /// Represents a role that can be assigned to users (e.g., User, Admin).
    
    public class Role
    {
        
        /// Primary key for the role record.
        
        public int Id { get; set; }

        
        /// Name of the role (unique semantic identifier used for authorization checks).
        
        [Required]
        public string RoleName { get; set; } = string.Empty;

        
        /// Navigation collection for the many-to-many relation to users via `UserRole`.
        
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
