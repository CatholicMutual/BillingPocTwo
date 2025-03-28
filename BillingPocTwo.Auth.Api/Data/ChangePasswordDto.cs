namespace BillingPocTwo.Auth.Api.Data
{
    public class ChangePasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
