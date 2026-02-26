using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
   
    /// Represents an application user.
    /// Contains authentication fields, status flag and navigation collections for related entities.
   
    public class User
    {
       
        /// Primary key for the user record.
       
        public int Id { get; set; }

       
        /// Unique username used for authentication.
       
        [Required]
        public string Username { get; set; } = string.Empty;

       
        /// Hashed password string. Use `PasswordHasher` utilities to create/verify hashes.
       
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

       
        /// Indicates if the account is active. Inactive accounts are prevented from logging in.
       
        public bool IsActive { get; set; } = true;

       
        /// Roles assigned to the user (many-to-many relationship via UserRole).
       
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

       
        /// Collection of expenses created by this user.
       
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

       
        /// Menu assignments for the user (authorization-driven navigation items).
       
        public ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
    }
}
