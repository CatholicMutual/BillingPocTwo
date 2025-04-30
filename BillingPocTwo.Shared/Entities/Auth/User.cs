namespace BillingPocTwo.Shared.Entities.Auth
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public bool Active { get; set; }
        public bool ServiceUser { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
        public bool ChangePasswordOnFirstLogin { get; set; }
        public string CreatedBy { get; set; } // New field
        public DateTime CreatedAt { get; set; } // New field
        public string? ModifiedBy { get; set; } // New field
        public DateTime? ModifiedAt { get; set; } // New field
        public ICollection<UserUserRole> UserUserRoles { get; set; }
    }
}
