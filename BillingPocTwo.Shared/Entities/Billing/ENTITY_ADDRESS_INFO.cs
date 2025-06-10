using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Billing
{
    public class ENTITY_ADDRESS_INFO
    {
        public decimal SYSTEM_ENTITY_CODE { get; set; }
        public string ADDRESS_TYPE { get; set; }
        public string? ADDRESS1 { get; set; }
        public string? ADDRESS2 { get; set; }
        public string? CITY { get; set; }
        public string? STATE { get; set; }
        public string? ZIP_CODE { get; set; }
        public string? COUNTRY { get; set; }
        public string FULL_NAME { get; set; }
        [Key]
        public decimal SEQ_ENTITY_ADDRESS_INFO { get; set; }
    }
}
