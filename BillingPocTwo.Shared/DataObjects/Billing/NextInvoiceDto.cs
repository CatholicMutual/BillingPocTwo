using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.DataObjects.Billing
{
    public class NextInvoiceDto
    {
        public decimal NextInvoiceAmount { get; set; }
        public DateTime? NextInvoiceDate { get; set; }
        public DateTime? NextInvoiceDueDate { get; set; }
    }
}
