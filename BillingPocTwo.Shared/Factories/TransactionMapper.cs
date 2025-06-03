using BillingPocTwo.Shared.Entities.Billing;
using BillingPocTwo.Shared.Entities.Transactions;
using BillingPocTwo.Shared.Enums;
using BillingPocTwo.Shared.Interfaces;

namespace BillingPocTwo.Shared.Factories
{
    public static class TransactionMapper
    {
        public static ITransaction Map(TRANSACTION_LOG log)
        {
            if (!Enum.TryParse<OriginalTransactionType>(log.TRANSACTION_TYPE, out var type))
                throw new ArgumentException($"Unknown transaction type: {log.TRANSACTION_TYPE}");

            switch (type)
            {
                case OriginalTransactionType.PAYMENT:
                case OriginalTransactionType.PAYMENT_ADJUSTMENT:
                    return new Payment
                    {
                        Type = TransactionType.Payment,
                        EntryDate = log.CREATED_ON ?? DateTime.MinValue,
                        EffectiveDate = log.TRANSACTION_EFF_DATE,
                        UserId = log.CREATED_BY ?? "",
                        AccountId = log.ACCOUNT_SYSTEM_CODE.ToString(),
                        ReceivedAmount = 0, // Set if you have an amount field
                        OriginalTransactionDescription = log.TRANSACTION_TYPE.ToString(),
                        PolicyNo = string.IsNullOrWhiteSpace(log.POLICY_NO) ? string.Empty : log.POLICY_NO
                    };
                case OriginalTransactionType.BILL:
                case OriginalTransactionType.BILL_DUE:
                case OriginalTransactionType.EARNED_BILL:
                    return new Invoice
                    {
                        Type = TransactionType.Invoice,
                        EntryDate = log.CREATED_ON ?? DateTime.MinValue,
                        EffectiveDate = log.TRANSACTION_EFF_DATE,
                        UserId = log.CREATED_BY ?? "",
                        AccountId = log.ACCOUNT_SYSTEM_CODE.ToString(),
                        Amount = 0, // Set if you have an amount field
                        OriginalTransactionDescription = log.TRANSACTION_TYPE.ToString(),
                        PolicyNo = string.IsNullOrWhiteSpace(log.POLICY_NO) ? string.Empty : log.POLICY_NO
                    };
                case OriginalTransactionType.ENDORSEMENT:
                    return new Endorsement
                    {
                        Type = TransactionType.Endorsement,
                        EntryDate = log.CREATED_ON ?? DateTime.MinValue,
                        EffectiveDate = log.TRANSACTION_EFF_DATE,
                        ExpiryDate = log.TRANSACTION_EXPIRY_DATE ?? DateTime.MinValue,
                        UserId = log.CREATED_BY ?? "",
                        AccountId = log.ACCOUNT_SYSTEM_CODE.ToString(),
                        // Set other amounts if available
                        OriginalTransactionDescription = log.TRANSACTION_TYPE.ToString(),
                        PolicyNo = string.IsNullOrWhiteSpace(log.POLICY_NO) ? string.Empty : log.POLICY_NO
                    };
                case OriginalTransactionType.NONMONEY_ENDORSEMENT:
                    return new NonMoneyEndorsement
                    {
                        Type = TransactionType.NonMoneyEndorsement,
                        EntryDate = log.CREATED_ON ?? DateTime.MinValue,
                        EffectiveDate = log.TRANSACTION_EFF_DATE,
                        ExpiryDate = log.TRANSACTION_EXPIRY_DATE ?? DateTime.MinValue,
                        UserId = log.CREATED_BY ?? "",
                        AccountId = log.ACCOUNT_SYSTEM_CODE.ToString(),
                        OriginalTransactionDescription = log.TRANSACTION_TYPE.ToString(),
                        PolicyNo = string.IsNullOrWhiteSpace(log.POLICY_NO) ? string.Empty : log.POLICY_NO
                    };
                case OriginalTransactionType.PAYMENT_TRANSFER_INTERNAL:
                    return new PaymentTransferInternal
                    {
                        Type = TransactionType.PaymentTransferInternal,
                        EntryDate = log.CREATED_ON ?? DateTime.MinValue,
                        EffectiveDate = log.TRANSACTION_EFF_DATE,
                        UserId = log.CREATED_BY ?? "",
                        AccountId = log.ACCOUNT_SYSTEM_CODE.ToString(),
                        ReceivedAmount = 0, // Set if you have an amount field
                        OriginalTransactionDescription = log.TRANSACTION_TYPE.ToString(),
                        PolicyNo = string.IsNullOrWhiteSpace(log.POLICY_NO) ? string.Empty : log.POLICY_NO
                    };
                case OriginalTransactionType.RENEWAL:
                    return new Renewal
                    {
                        Type = TransactionType.Renewal,
                        EntryDate = log.CREATED_ON ?? DateTime.MinValue,
                        EffectiveDate = log.TRANSACTION_EFF_DATE,
                        ExpiryDate = log.TRANSACTION_EXPIRY_DATE ?? DateTime.MinValue,
                        UserId = log.CREATED_BY ?? "",
                        AccountId = log.ACCOUNT_SYSTEM_CODE.ToString(),
                        // Set other amounts if available
                        OriginalTransactionDescription = log.TRANSACTION_TYPE.ToString(),
                        PolicyNo = string.IsNullOrWhiteSpace(log.POLICY_NO) ? string.Empty : log.POLICY_NO
                    };
                default:
                    return new Transaction
                    {
                        Type = TransactionType.Transaction,
                        EntryDate = log.CREATED_ON ?? DateTime.MinValue,
                        EffectiveDate = log.TRANSACTION_EFF_DATE,
                        UserId = log.CREATED_BY ?? "",
                        AccountId = log.ACCOUNT_SYSTEM_CODE.ToString(),
                        OriginalTransactionDescription = log.TRANSACTION_TYPE.ToString(),
                        PolicyNo = string.IsNullOrWhiteSpace(log.POLICY_NO) ? string.Empty : log.POLICY_NO
                    };
            }
        }
    }
}
