using Speech2TextPrototype.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Models
{
    public class LookupOutputModel
    {
        public List<string> measures { get; set; }
        public List<string> dates { get; set; }
        public string querySql { get; set; }
        public double scalarValue { get; set; }
    }
}
