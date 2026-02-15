namespace ExpenseTracker.Models
{
    public class UserMenu
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int MenuId { get; set; }

        public User? User { get; set; }

        public Menu? Menu { get; set; }
    }
}
