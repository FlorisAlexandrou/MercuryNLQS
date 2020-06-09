using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Data
{
    public class LookupValues
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public string WhereStmt { get; set; }
        public string WhereType { get; set; }
    }
}
