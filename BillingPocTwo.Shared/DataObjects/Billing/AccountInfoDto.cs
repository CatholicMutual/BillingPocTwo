using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.DataObjects.Billing
{
    public class AccountInfoDto
    {
        public decimal? PAST_DUE_AMT { get; set; }
        public decimal? CURRENT_MIN_AMT { get; set; }
        public decimal? BALANCE { get; set; }
    }
}
