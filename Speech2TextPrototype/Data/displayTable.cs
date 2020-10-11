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
        public string PRODUCT_NAME { get; set; }
        public DateTime? PERIOD_START { get; set; }
        public int? M_SALES_ITEMS { get; set; }
        public double? M_SALES_VALUE { get; set; }
        public double? M_SALES_VOLUME { get; set; }
    }
}
