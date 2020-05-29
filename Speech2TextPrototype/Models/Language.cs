using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using Newtonsoft.Json;

namespace Speech2TextPrototype.Models
{
    public class Language
    {
        [JsonProperty(PropertyName = "text")]
        public string text { get; set; }

        [JsonProperty(PropertyName = "entities")]
        public Entity[] entities { get; set; }

    }

    public class Entity
    {

        public Entity(string text, string category, string subCategory, int length, double confidence)
        {
            this.text = text;
            this.category = category;
            subcategory = subCategory;
            this.length = length;
            this.confidence = confidence;
        }

        public string text { get; set; }
        public string category { get; set; }
        public string subcategory { get; set; }
        public int length { get; set; }
        public double confidence { get; set; }
    }
}
