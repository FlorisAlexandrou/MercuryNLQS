using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Models
{
    public class PyRes
    {
        [JsonProperty(PropertyName = "query")]
        public string query { get; set; }
        [JsonProperty(PropertyName = "tokens")]
        public string[] tokens { get; set; }
    }
}
