using BillingPocTwo.Shared.Enums;
using BillingPocTwo.Shared.Interfaces;

namespace BillingPocTwo.Shared.Entities.Transactions
{
    public class PaymentTransferInternal : ITransaction
    {
        public TransactionType Type { get; set; } = TransactionType.PaymentTransferInternal;
        public DateTime EntryDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string UserId { get; set; }
        public string AccountId { get; set; }
        public decimal ReceivedAmount { get; set; }
        public string? OriginalTransactionDescription { get; set; }
        public string? PolicyNo { get; set; }
        public decimal? TransactionId { get; set; }
        public decimal? Amount { get; set; }
    }
}
