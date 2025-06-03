using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Billing
{
    public class ASSIGNED_PAYMENT
    {
        [Key]
        public decimal PAYMENT_ITEM_SEQ { get; set; }
        public decimal SYSTEM_TRANSACTION_SEQ { get; set; }
        public string? TRANSACTION_TYPE { get; set; }
        public decimal? PAYMENT_AMOUNT { get; set; }
        public string? CREATED_BY { get; set; }
        public string? USER_REMARK { get; set; }
        public decimal? POLICY_ID { get; set; }
        public string? ACCOUNT_NO { get; set; }
    }
}
