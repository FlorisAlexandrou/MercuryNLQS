using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;

namespace Speech2TextPrototype.Models
{
    public class Language
    {
        public string text { get; set; }
        public Entity[] entities { get; set; }

        public override string ToString() => JsonSerializer.Serialize<Language>(this);
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
