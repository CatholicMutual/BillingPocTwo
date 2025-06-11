using BillingPocTwo.Shared.Enums;
using BillingPocTwo.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Transactions
{
    public class Transaction : ITransaction
    {
        public TransactionType Type { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string? OriginalTransactionDescription { get; set; } = string.Empty;
        public string? PolicyNo { get; set; }
        public decimal? TransactionId { get; set; }
        public decimal? Amount { get; set; }
    }
}
