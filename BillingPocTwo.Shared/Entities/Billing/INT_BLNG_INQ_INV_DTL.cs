using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Billing
{
    public class INT_BLNG_INQ_INV_DTL
    {
        [Key]
        public decimal AUTO_KEY { get; set; }
        public decimal? BILLING_INQ_REQ_SEQ { get; set; }
        public decimal? ID_KEY { get; set; }
        public decimal? REQ_SEQ_SERIAL_NO { get; set; }
        public decimal? REQ_SEQ_SUB_SERIAL_NO { get; set; }
        public decimal? ADJ_AMT_AFTER_LAST_INV { get; set; }
        public decimal? BALANCE { get; set; }
        public decimal? CURRENT_MIN_DUE { get; set; }
        public decimal? LAST_INVOICE_AMT { get; set; }
        public DateTime? LAST_INVOICE_DATE { get; set; }
        public DateTime? LAST_INVOICE_DUE_DATE { get; set; }
        public DateTime? NEXT_INST_DATE { get; set; }
        public decimal? NEXT_INST_DUE_AMT { get; set; }
        public DateTime? NEXT_INS_DUE_DATE { get; set; }
        public decimal? PAST_DUE_AMT { get; set; }
        public decimal? PAY_RECVD_AFT_LAST_INV { get; set; }
        public decimal? PLEASE_PAY_AMT { get; set; }
        public decimal? TOTAL_ADJUSTMENT_AMT { get; set; }
        public decimal? TOTAL_PAYMENT_AMT { get; set; }
        public decimal? TOTAL_RECV_AMT { get; set; }
        public decimal? POLICY_TERM_ID { get; set; }
        public decimal? ACCOUNT_SYSTEM_CODE { get; set; }
    }
}
