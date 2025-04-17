namespace BillingPocTwo.Shared.Entities.Auth
{
    public class UserState
    {
        public bool IsAuthenticated { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; } = new List<string>(); // Store roles as strings (ROLE_ID or ROLE_DESCRIPTION)
        public bool IsAdmin { get; set; }
        public bool IsUser { get; set; }
    }
}
