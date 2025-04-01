namespace BillingPocTwo.Shared.Entities
{
    public class UserState
    {
        public bool IsAdmin { get; set; } = false;
        public bool IsUser { get; set; } = false;
        public bool IsAuthenticated { get; set; } = false;
        public string? Email { get; set; }
    }
}
