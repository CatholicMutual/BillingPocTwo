using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Auth
{
    public class UserUserRole
    {
        public Guid UserId { get; set; } // Foreign key to User
        public Guid RoleId { get; set; } // Foreign key to UserRole

        public User User { get; set; } // Navigation property
        public UserRole Role { get; set; } // Navigation property
    }

}
