using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Billing
{
    public class POLICY_REGISTER
    {
        [Key]
        public decimal POLICY_TERM_ID { get; set; }
        public string? POLICY_NO { get; set; }
        public decimal POLICY_RENEW_NO { get; set; }
        public decimal INBOUND_TRANSACTION_SEQ { get; set; }
        public decimal? SYSTEM_TRANSACTION_SEQ { get; set; }
        public string PRODUCT_CODE { get; set; }
        public string STATE_CODE { get; set; }
        public string OPERATING_COMPANY { get; set; }
        public string CRT_CODE { get; set; }
        public decimal? ACCOUNT_SYSTEM_CODE { get; set; }
        public decimal BROKER_SYSTEM_CODE { get; set; }
        public decimal INSURED_SYSTEM_CODE { get; set; }
        public string PAYMENT_PLAN { get; set; }
        public string APPLICATION_NO { get; set; }
        public DateTime POLICY_EFFECTIVE_DATE { get; set; }
        public DateTime POLICY_EXPIRATION_DATE { get; set; }
        public string LEGAL_STATUS { get; set; }
        public decimal BILL_TO_SYSTEM_CODE { get; set; }
        public decimal POLICY_ID { get; set; }
        public DateTime? EQUITY_DATE { get; set; }
        public string? COUNTRY { get; set; }
        public string? SOURCE_POLICY_ID { get; set; }
        public decimal? BALANCE { get; set; }
        public Guid ROWID { get; set; }
        public string? SUB_BILLTYPE { get; set; }
    }
}
