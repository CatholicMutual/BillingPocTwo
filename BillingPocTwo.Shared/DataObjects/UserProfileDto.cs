using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.DataObjects
{
    public class UserProfileDto
    {
        public string Email { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Active { get; set; }
        public bool ServiceUser { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
