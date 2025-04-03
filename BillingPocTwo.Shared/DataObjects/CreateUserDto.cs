namespace BillingPocTwo.Shared.DataObjects
{
    public class CreateUserDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string? CreatedBy { get; set; } // New field
        public DateTime CreatedAt { get; set; } // New field
    }
}
