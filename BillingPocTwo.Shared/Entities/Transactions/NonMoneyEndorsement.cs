using BillingPocTwo.Shared.Enums;
using BillingPocTwo.Shared.Interfaces;

namespace BillingPocTwo.Shared.Entities.Transactions
{
    public class NonMoneyEndorsement : ITransaction
    {
        public TransactionType Type { get; set; } = TransactionType.NonMoneyEndorsement;
        public DateTime EntryDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string UserId { get; set; }
        public string AccountId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? OriginalTransactionDescription { get; set; }
        public string? PolicyNo { get; set; }
        public decimal? PolicyTermId { get; set; }
        public decimal? TransactionId { get; set; }
        public decimal? Amount { get; set; }
    }
}
