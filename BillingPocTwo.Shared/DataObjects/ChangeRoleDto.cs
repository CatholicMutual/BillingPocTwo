namespace BillingPocTwo.Shared.DataObjects
{
    public class ChangeRoleDto
    {
        public string Email { get; set; }
        public List<string> NewRoles { get; set; } = new List<string>();
    }
}
