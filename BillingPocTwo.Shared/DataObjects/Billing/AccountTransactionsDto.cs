using BillingPocTwo.Shared.Entities.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.DataObjects.Billing
{
    public class AccountTransactionsDto
    {
        public List<TRANSACTION_LOG> TransactionLogs { get; set; } = new();
        public List<ASSIGNED_PAYMENT> AssignedPayments { get; set; } = new();

    }
}
