using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.DataObjects.Billing
{
    public class LastInvoiceDto
    {
        public decimal LastInvoiceAmount { get; set; }
        public DateTime? LastInvoiceDate { get; set; }
        public DateTime? LastInvoiceDueDate { get; set; }
    }
}
