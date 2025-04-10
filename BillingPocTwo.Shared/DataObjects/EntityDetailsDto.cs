using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Shared.DataObjects
{
    public class EntityDetailsDto
    {
        public decimal SYSTEM_ENTITY_CODE { get; set; }
        public string? FULL_NAME { get; set; }
        public string? CITY { get; set; }
        public string? STATE { get; set; }
        public List<decimal> PolicyTermIds { get; set; } = new();

    }
}
