using BillingPocTwo.Shared.Enums;

namespace BillingPocTwo.Shared.Interfaces
{
    public interface ITransaction
    {
        TransactionType Type { get; set; }
        DateTime EntryDate { get; set; }
        DateTime EffectiveDate { get; set; }
        string UserId { get; set; }
        string AccountId { get; set; }
        string? OriginalTransactionDescription { get; set; }
        string? PolicyNo { get; set; }
        decimal? TransactionId { get; set; }
        decimal? Amount { get; set; }
    }
}
