using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.DataObjects.Billing
{
    public class TransactionDto
    {
        public decimal SYSTEM_TRANSACTION_SEQ { get; set; }
        public decimal? POLICY_TERM_ID { get; set; }
        public string? POLICY_NO { get; set; }
        public string TRANSACTION_TYPE { get; set; }
        public DateTime? CREATED_ON { get; set; }
        //public string? ACTION { get; set; }
        public DateTime TRANSACTION_EFF_DATE { get; set; }
        public DateTime? TRANSACTION_EXPIRY_DATE { get; set; }
        //public decimal AMOUNT { get; set; }
        //public decimal BALANCE { get; set; }
        public string? CREATED_BY { get; set; }
        //public string? SOURCE_SYSTEM_ENTITY_CODE { get; set; }
    }
}
