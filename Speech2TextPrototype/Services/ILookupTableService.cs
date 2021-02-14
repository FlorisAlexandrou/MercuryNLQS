using Consumer_Retail_Research_Analytics_NLP.Models;
using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;
using System.Collections.Generic;

namespace Speech2TextPrototype.Services
{
    public interface ILookupTableService
    {
        public SqlAnswer Token2Sql(PyRes res);

        public string GroupByFilters(string query, string groupByFilter, string uuid);

        public List<string> GetSpeechRecognitionCustomWords();
    }
}
