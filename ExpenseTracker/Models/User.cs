using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    /// <summary>
    /// Represents an application user.
    /// Contains authentication fields, status flag and navigation collections for related entities.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key for the user record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique username used for authentication.
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password string. Use `PasswordHasher` utilities to create/verify hashes.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the account is active. Inactive accounts are prevented from logging in.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Roles assigned to the user (many-to-many relationship via UserRole).
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Collection of expenses created by this user.
        /// </summary>
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        /// <summary>
        /// Menu assignments for the user (authorization-driven navigation items).
        /// </summary>
        public ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
    }
}
