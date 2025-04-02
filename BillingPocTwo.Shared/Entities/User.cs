namespace BillingPocTwo.Shared.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
        public bool ChangePasswordOnFirstLogin { get; set; }
    }
}
