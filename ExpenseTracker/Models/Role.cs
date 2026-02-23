using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    /// <summary>
    /// Represents a role that can be assigned to users (e.g., User, Admin).
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Primary key for the role record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the role (unique semantic identifier used for authorization checks).
        /// </summary>
        [Required]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Navigation collection for the many-to-many relation to users via `UserRole`.
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
