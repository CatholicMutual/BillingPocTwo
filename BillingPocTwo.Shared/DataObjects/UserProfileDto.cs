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
        public List<string> Roles { get; set; } = new List<string>();
    }
}
