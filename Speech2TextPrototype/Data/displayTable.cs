using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Data
{
    public class DisplayTable
    {
        public int? ROW { get; set; }
        public string BRAND { get; set; }
        public string CATEGORY_NAME { get; set; }
        public DateTime? PERIOD_START { get; set; }
        public int? M_SALES_ITEMS { get; private set; }
        public double? M_SALES_VALUE { get; private set; }
        public double? M_SALES_VOLUME { get; private set; }
    }
}
