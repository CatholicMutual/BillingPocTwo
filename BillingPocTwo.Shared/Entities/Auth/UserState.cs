namespace BillingPocTwo.Shared.Entities.Auth
{
    public class UserState
    {
        public bool IsAdmin { get; set; } = false;
        public bool IsUser { get; set; } = false;
        public bool IsAuthenticated { get; set; } = false;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
