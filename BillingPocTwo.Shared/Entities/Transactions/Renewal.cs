using BillingPocTwo.Shared.Enums;
using BillingPocTwo.Shared.Interfaces;

namespace BillingPocTwo.Shared.Entities.Transactions
{
    public class Renewal : ITransaction
    {
        public TransactionType Type { get; set; } = TransactionType.Renewal;
        public DateTime EntryDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string UserId { get; set; }
        public string AccountId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DeferredAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceiveableAmount { get; set; }
        public string? OriginalTransactionDescription { get; set; }
        public string? PolicyNo { get; set; }
        public decimal? TransactionId { get; set; }
        public decimal? Amount { get; set; }
    }
}
