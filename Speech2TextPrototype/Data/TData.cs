using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Data
{
    public class TData
    {
        [Key]
        [Column(TypeName = "bigint")]
        public int TID { get; set; }
        public int? QUANTITY { get; set; }
        public double? PRICE { get; set; }
        public int? CATEGORY_ID{ get; set; }
        public string CATEGORY_NAME { get; set; }
        public string UNIT_MEASUREMENT { get; set; }
        public int? PRODUCT_ID { get; set; }
        public string PRODUCT_NAME { get; set; }
        public string BRAND { get; set; }
        public double? SIZE { get; set; }
        public string SIZE_DETAILS { get; set; }
        public int? PERIOD_ID { get; set; }
        public DateTime? PERIOD_START { get; set; }
        public DateTime? PERIOD_END { get; set; }
        public int? OUTLET_ID { get; set; }
        public string OUTLET_NAME { get; set; }
        public int? OUTLET_TYPE_ID { get; set; }
        public string OUTLET_TYPE_NAME { get; set; }
        public int? AREA_ID { get; set; }
        public string AREA_NAME { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? M_SALES_ITEMS { get; private set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? M_SALES_VALUE { get; private set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? M_SALES_VOLUME { get; private set; }
    }

    public class SalesValue
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? M_SALES_VALUE { get; private set; }
    }
}
