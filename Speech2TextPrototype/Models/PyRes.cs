using Newtonsoft.Json;

namespace Speech2TextPrototype.Models
{
    public class PyRes
    {
        [JsonProperty(PropertyName = "query")]
        public string query { get; set; }
        [JsonProperty(PropertyName = "tokens")]
        public string[] tokens { get; set; }
        [JsonProperty(PropertyName = "bigrams")]
        public string[] bigrams { get; set; }
        [JsonProperty(PropertyName = "trigrams")]
        public string[] trigrams { get; set; }
        [JsonProperty(PropertyName = "isSqlQuery")]
        public bool isSqlQuery { get; set; }
    }
}
