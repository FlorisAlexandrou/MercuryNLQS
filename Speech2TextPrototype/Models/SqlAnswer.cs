using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consumer_Retail_Research_Analytics_NLP.Models
{
    public class SqlAnswer
    {
        public List<string> measures { get; set; }
        public string sqlQuery { get; set; }
        public double scalar { get; set; }
        public string error { get; set; }
    }
}
