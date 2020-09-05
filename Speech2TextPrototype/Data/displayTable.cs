using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Data
{
    public class DisplayTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ROW { get; set; }
        public string BRAND { get; set; }
        public string CATEGORY_NAME { get; set; }
        public DateTime? PERIOD_START { get; set; }
        public int? QUANTITY { get; set; }
        public double? PRICE { get; set; }
        public double? SIZE { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? M_SALES_ITEMS { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? M_SALES_VALUE { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? M_SALES_VOLUME { get; set; }
    }
}
