using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.Entities.Billing
{
    public class ENTITY_REGISTER
    {
        [Key]
        public decimal SYSTEM_ENTITY_CODE { get; set; }
        public string ENTITY_TYPE { get; set; }
        public string? SOURCE_SYSTEM_ENTITY_CODE { get; set; }
        public decimal? BALANCE { get; set; }
    }
}
