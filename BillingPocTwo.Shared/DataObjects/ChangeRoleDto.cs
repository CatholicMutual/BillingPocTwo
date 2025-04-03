namespace BillingPocTwo.Shared.DataObjects
{
    public class ChangeRoleDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Active { get; set; }
        public bool ServiceUser { get; set; }
        public List<string> NewRoles { get; set; } = new List<string>();
    }
}
