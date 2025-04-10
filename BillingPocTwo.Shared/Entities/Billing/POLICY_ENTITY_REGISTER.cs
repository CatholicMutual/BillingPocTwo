using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Billing
{
    public class POLICY_ENTITY_REGISTER
    {
        [Key]
        public decimal POLICY_TERM_ID { get; set; }
        [Key]
        public decimal SYSTEM_ENTITY_CODE { get; set; }
        [Key]
        public string SYSTEM_ACTIVITY_NO { get; set; }
        [Key]
        public decimal SYSTEM_TRANSACTION_SEQ { get; set; }
        public decimal? ENTITY_TRANSACTION_SEQ { get; set; }
        [Key]
        public string ENTITY_TYPE { get; set; }
        public string BILLING_ENTITY_YN { get; set; }
        public string? LOCATION_ID { get; set; }
    }
}
