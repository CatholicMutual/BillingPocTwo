using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Auth
{
    public class ROLE_MASTER
    {
        [Key]
        public decimal SEQ_ROLE_MASTER { get; set; }
        public string ROLE_ID { get; set; } = string.Empty;
        public string ROLE_DESCRIPTION { get; set; } = string.Empty;
        public string IS_LOCKED { get; set; } = string.Empty;
        public string? LOCKED_REASON { get; set; } = string.Empty;
        public DateTime? EFFECTIVE_DATE { get; set; }
        public DateTime? EXPIRY_DATE { get; set; }
        public long? CREATED_BY { get; set; }
        public DateTime? CREATED_ON { get; set; }
        public long? MODIFIED_BY { get; set; }
        public DateTime? MODIFIED_ON { get; set; }
        public Guid ROWID { get; set; }
    }
}
