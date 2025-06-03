using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Billing
{
    public class TRANSACTION_LOG
    {
        [Key]
        public decimal SYSTEM_TRANSACTION_SEQ { get; set; }
        public decimal? POLICY_TERM_ID { get; set; }
        public string? POLICY_NO { get; set; }
        public string TRANSACTION_TYPE { get; set; }
        public string? PAYMENT_METHOD { get; set; }
        public DateTime? CREATED_ON { get; set; }
        //public string? ACTION { get; set; }
        public DateTime TRANSACTION_EFF_DATE { get; set; }
        public DateTime? SOURCE_SYSTEM_PROCESS_DATE { get; set; }
        public DateTime? TRANSACTION_EXPIRY_DATE { get; set; }
        public decimal ACCOUNT_SYSTEM_CODE { get; set; }
        //public decimal BALANCE { get; set; }
        public string? CREATED_BY { get; set; }
        //public string? SOURCE_SYSTEM_ENTITY_CODE { get; set; }
    }
}
