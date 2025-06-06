﻿namespace BillingPocTwo.Shared.Enums
{
    public enum OriginalTransactionType
    {
        PAYMENT,
        PAYMENT_ADJUSTMENT,
        NONMONEY_ENDORSEMENT,
        ENDORSEMENT,
        BILL_DUE,
        BILL,
        EARNED_BILL,
        PAYMENT_TRANSFER_INTERNAL,
        RENEWAL,
        EXPIRY,
        REFUND,
        NEW,
        AUDIT,
        CANCELLATION,
        WRITEOFF,
        PAYMENT_TRANSFER_EXTERNAL,
        RETURNED_PAYMENT,
        CHANGE_SCHEDULE,
        REINSTATEMENT,
        ACCOUNT_EP_INVOICE,
        BACKOUT,
        BILLING_ENDORSEMENT,
        BILLING_HOLD,
        BILLING_RELEASE,
        BINDER,
        BINDER_BACKOUT,
        CHANGE_INSTALLMENT_SCHEDULE,
        DEMAND_NOTICE,
        HOLD,
        INTERIM_AUDIT,
        PAST_DUE,
        PAYPLAN_ENDORSEMENT,
        RELEASE,
        RESCISSION,
        RETURN_COMMISSION,
        VOID_REFUND
    }
}