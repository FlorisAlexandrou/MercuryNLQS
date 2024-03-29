﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Speech2TextPrototype.Data
{
    public class DisplayTable
    {
        public string UUID { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ROWNUMBER { get; set; }
        public string BRAND { get; set; }
        public string CATEGORY_NAME { get; set; }
        public string PRODUCT_NAME { get; set; }
        public DateTime? PERIOD_START { get; set; }
        public int? M_SALES_ITEMS { get; set; }
        public double? M_SALES_VALUE { get; set; }
        public double? M_SALES_VOLUME { get; set; }
    }
}
