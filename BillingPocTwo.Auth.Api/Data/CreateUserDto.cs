namespace BillingPocTwo.Auth.Api.Data
{
    public class CreateUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
