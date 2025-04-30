namespace BillingPocTwo.Shared.Entities.Auth
{
    public class UserRole
    {
        public Guid Id { get; set; } // Primary key
        public Guid UserId { get; set; } // Foreign key to User
        public string RoleName { get; set; } // Role name or ID

        public User User { get; set; } // Navigation property
    }

}
