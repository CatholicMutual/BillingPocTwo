using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.DataObjects
{
    public class AccountDetailsDto
    {
        public decimal SYSTEM_ENTITY_CODE { get; set; }
        public string SOURCE_SYSTEM_ENTITY_CODE { get; set; } = string.Empty;
        public string DOING_BUSINESS_AS_NAME { get; set; }
        public string ADDRESS1 { get; set; }
        public string ADDRESS2 { get; set; }
        public string CITY { get; set; }
        public string STATE { get; set; }
        public string ZIP_CODE { get; set; }
        public string FULL_NAME { get; set; }
        public List<decimal> PolicyTermIds { get; set; }
    }
}
